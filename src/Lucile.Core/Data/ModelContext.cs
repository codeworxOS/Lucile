using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Lucile.Data.Metadata;
using Lucile.Data.Tracking;

namespace Lucile.Data
{
    public class ModelContext : IDisposable
    {
        private static readonly MethodInfo _enumerableJoinMethod;

        private static readonly object EntityInfoLocker = new object();

        private static readonly Dictionary<EntityMetadata, EntityInfo> EntityInfos = new Dictionary<EntityMetadata, EntityInfo>();

        private static readonly ConcurrentDictionary<NavigationPropertyMetadata, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<object>>> ReverseFixupCache;

        private readonly Dictionary<AttachTransaction, DateTime> _attachTransactions;

        private readonly object _attachTransactionsLocker = new object();

        private readonly ManualResetEvent _attachTransactionsSignal;

        private readonly Timer _cleanupTimer;

        private readonly Dictionary<EntityTransaction, DateTime> _entityTransactions;

        private readonly object _entityTransactionsLocker = new object();

        private readonly ManualResetEvent _entityTransactionsSignal;

        private readonly MetadataModel _model;

        private readonly Dictionary<EntityMetadata, Dictionary<object, System.WeakReference<object>>> _trackedObjects = new Dictionary<EntityMetadata, Dictionary<object, System.WeakReference<object>>>();

        private readonly ITrackingInfoProvider _trackingInfoProvider;

        private readonly object _updateLocker = new object();

        private bool _disposedValue = false;

        static ModelContext()
        {
            Expression<Func<IEnumerable<object>, IEnumerable<object>, Func<object, object>, Func<object, object>, Func<object, object, object>, IEnumerable<object>>> enumerableJoinExpression = (a, b, c, d, e) => a.Join(b, c, d, e);
            _enumerableJoinMethod = ((MethodCallExpression)enumerableJoinExpression.Body).Method.GetGenericMethodDefinition();

            ReverseFixupCache = new ConcurrentDictionary<NavigationPropertyMetadata, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<object>>>();
        }

        public ModelContext(MetadataModel model)
            : this(model, new TrackingInfoProvider())
        {
        }

        public ModelContext(MetadataModel model, ITrackingInfoProvider trackingInfoProvider)
        {
            _model = model;
            _trackingInfoProvider = trackingInfoProvider;
            _cleanupTimer = new Timer(WeakReferenceCleanupTimer, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            _attachTransactions = new Dictionary<AttachTransaction, DateTime>();
            _entityTransactions = new Dictionary<EntityTransaction, DateTime>();
            _attachTransactionsSignal = new ManualResetEvent(true);
            _entityTransactionsSignal = new ManualResetEvent(true);
        }

        public IEnumerable<object> TrackedObjects
        {
            get
            {
                IEnumerable<object> result;
                lock (_updateLocker)
                {
                    result = (from tracked in _trackedObjects
                              from keys in tracked.Value
                              let item = keys.Value.TargetOrDefault()
                              where item != null
                              select item).ToList();
                }

                return result;
            }
        }

        public IEnumerable<T> Attach<T>(IEnumerable<T> items, MergeStrategy mergeStrategy = MergeStrategy.UpdateIfUnchanged)
            where T : class
        {
            using (BeginAttach<T>())
            {
                return DoClean(items, mergeStrategy, out var totalCleaned);
            }
        }

        public T AttachSingle<T>(T item, MergeStrategy mergeStrategy = MergeStrategy.UpdateIfUnchanged)
            where T : class
        {
            // null-Objekt kann nicht hinzugefügt werden
            if (item == null)
            {
                return null;
            }

            return Attach(new[] { item }, mergeStrategy).First();
        }

        public EntityTransaction BeginTransaction<T>(T entity)
            where T : class
        {
            _attachTransactionsSignal.WaitOne();
            var transaction = new EntityTransaction(this, new[] { entity });
            lock (_entityTransactionsLocker)
            {
                _entityTransactions.Add(transaction, DateTime.Now);
                _entityTransactionsSignal.Reset();
            }

            return transaction;
        }

        public void Clear()
        {
            lock (_updateLocker)
            {
                _trackedObjects.Clear();
            }
        }

        public bool Contains(object item)
        {
            var entity = _model.GetEntityMetadata(item);
            var key = entity?.GetPrimaryKeyObject(item);
            if (key != null)
            {
                var info = GetEntityInfo(entity);
                return GetTrackedObjectOrDefault(info, key) != null;
            }

            return false;
        }

        public IEnumerable<T> Detach<T>(IEnumerable<T> items)
            where T : class
        {
            var result = new List<T>();

            using (BeginAttach<T>())
            {
                var dict = new ConcurrentDictionary<EntityMetadata, Dictionary<object, object>>();

                foreach (var item in items)
                {
                    var entity = _model.GetEntityMetadata(item);
                    var entities = dict.GetOrAdd(entity, p => new Dictionary<object, object>());
                    entities.Add(item, null);
                }

                foreach (var item in dict)
                {
                    var entityInfo = GetEntityInfo(item.Key);
                    ReplaceItems(entityInfo, item.Value);

                    result.AddRange(TryRemoveTrackedObjects<T>(entityInfo, item.Value.Keys.Select(p => entityInfo.EntityMetadata.GetPrimaryKeyObject(p))));
                }
            }

            return result;
        }

        public T DetachSingle<T>(T item)
        where T : class
        {
            return Detach(new[] { item }).FirstOrDefault();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void EndTransaction(EntityTransaction transaction)
        {
            lock (_entityTransactionsLocker)
            {
                this._entityTransactions.Remove(transaction);
                if (this._entityTransactions.Count == 0)
                {
                    this._entityTransactionsSignal.Set();
                }
            }
        }

        public IEnumerable<object> GetChanges()
        {
            IEnumerable<object> result;
            lock (_updateLocker)
            {
                result = (from tracked in _trackedObjects
                          from keys in tracked.Value
                          let item = keys.Value.TargetOrDefault()
                          let state = _trackingInfoProvider.GetState(item)
                          where item != null && state.HasValue && state.Value != Tracking.TrackingState.Unchanged
                          select item).ToList();
            }

            var changes = result.GroupBy(p => _model.GetEntityMetadata(p))
                                .ToDictionary(p => p.Key, p => p.Select(x => x).ToList());

            var sorted = _model.SortedByDependency();

            foreach (var item in GetChangesByState(sorted, changes, TrackingState.Added))
            {
                yield return item;
            }

            foreach (var item in GetChangesByState(sorted, changes, TrackingState.Modified))
            {
                yield return item;
            }

            foreach (var item in GetChangesByState(sorted.Reverse(), changes, TrackingState.Deleted))
            {
                yield return item;
            }
        }

        public AttachOperations<T> Merge<T>(IEnumerable<T> items, MergeStrategy mergeStrategy = MergeStrategy.UpdateIfUnchanged)
                                                                    where T : class
        {
            var itemsList = items.Where(p => p != null).ToList();

            using (BeginAttach<T>())
            {
                var resultItems = DoClean<T>(itemsList, mergeStrategy, out var totalTuples);

                var result = new AttachOperations<T>(resultItems);

                foreach (var item in totalTuples)
                {
                    if (item.Value.Added)
                    {
                        result.TrackAdded(item.Value.Target);
                    }

                    if (item.Value.GetProperties().Any())
                    {
                        result.TrackMerged(item.Value.Target, item.Value.GetProperties());
                    }
                }

                return result;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cleanupTimer.Dispose();
                }

                _disposedValue = true;
            }
        }

        private static void FillNavigationPropertyAndPrincipal(NavigationPropertyMetadata navProp, object source, object target)
        {
            ////using (new ChangeTrackingScope(ChangeTracking.Disable, source, target))
            ////{
            navProp.SetValue(source, target);

            // das geladene Property als geladen markieren
            ////source.MarkNavigationPropertyAsLoaded(navProp.Name);

            if (navProp.TargetNavigationProperty != null)
            {
                if (navProp.TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.Many)
                {
                    navProp.TargetNavigationProperty.AddItem(target, source);
                }
                else
                {
                    navProp.TargetNavigationProperty.SetValue(target, source);

                    // am Ziel markieren, dass es geladen ist
                    ////target.MarkNavigationPropertyAsLoaded(navProp.TargetNavigationProperty.Name);
                }
            }
        }

        private object AddTrackedObject(EntityInfo entityInfo, object key, object data)
        {
            bool added;
            return AddTrackedObject(entityInfo, key, data, out added);
        }

        private object AddTrackedObject(EntityInfo entityInfo, object key, object data, out bool added)
        {
            object test;

            System.WeakReference<object> result;

            lock (_updateLocker)
            {
                Dictionary<object, System.WeakReference<object>> first;
                if (!_trackedObjects.TryGetValue(entityInfo.RootType, out first))
                {
                    // EntityInfo noch überhaupt nicht in TrackedObjects
                    first = new Dictionary<object, System.WeakReference<object>> { { key, new System.WeakReference<object>(data) } };
                    _trackedObjects.Add(entityInfo.RootType, first);
                    ////data.ChangeTracker.IsAttached = true;
                    added = true;

                    return data;
                }

                if (!first.TryGetValue(key, out result))
                {
                    // Primärschlüssel wurde nicht gefunden
                    first.Add(key, new System.WeakReference<object>(data));
                    ////data.ChangeTracker.IsAttached = true;
                    added = true;

                    return data;
                }

                test = result.TargetOrDefault();
                if (test == null)
                {
                    // WeakReference könnte collected worden sein
                    result.SetTarget(data);
                    ////data.ChangeTracker.IsAttached = true;
                    added = true;

                    return data;
                }
            }

            // Objekt bereits gefunden
            added = false;

            return test;
        }

        private AttachTransaction BeginAttach<T>()
            where T : class
        {
            ////if (ObjectManagerScope.Current == null)
            ////{
            this._entityTransactionsSignal.WaitOne();
            ////}
            var transaction = new AttachTransaction(this);
            lock (_attachTransactionsLocker)
            {
                this._attachTransactions.Add(transaction, DateTime.Now);
                this._attachTransactionsSignal.Reset();
            }

            return transaction;
        }

        private bool Clean(
            object source,
            Dictionary<object, CleanupTuple> cleanedTuples,
            List<Tuple<object, NavigationPropertyMetadata>> fixups,
            MergeStrategy mergeStrategy,
            out CleanupTuple cleanedItem)
        {
            // already cleaned;
            if (cleanedTuples.TryGetValue(source, out cleanedItem))
            {
                return false;
            }

            // Metadaten für Source ermitteln
            var entity = _model.GetEntityMetadata(source);
            var entityInfo = GetEntityInfo(entity);
            object result = null;
            bool added = false;

            ////if (source.ChangeTracker.State == ObjectState.Added && !entity.IsPrimaryKeySet(source))
            ////{
            ////    // Neues Objekt ohne Primärschlüssel
            ////    if (!source.ChangeTracker.TemporaryObjectId.HasValue)
            ////    {
            ////        // temporäre Id vergeben
            ////        source.ChangeTracker.TemporaryObjectId = Guid.NewGuid();
            ////    }
            ////    result = AddTrackedObject(entityInfo, source.ChangeTracker.TemporaryObjectId, source, out added);

            ////    if (!added && result != source)
            ////    {
            ////        // Objekt wurde nicht hinzugefügt und nicht bereits mit der selben temporären Id gefunden
            ////        //throw new InvalidOperationException("object with same temporary id is already tracked!");
            ////        return result;
            ////    }
            ////}
            ////else if (source.ChangeTracker.State == ObjectState.Detached)
            ////{
            ////    // Objekt ist detached
            ////    var entityKey = entityInfo.RootType.GetPrimaryKeyObject(source);
            ////    //var key = source.ChangeTracker.TemporaryObjectId ?? entityKey;
            ////    var key = entityKey
            ////    // aus TrackedObjects über temporäre Id oder Primärschlüssel entfernen
            ////    if (key != null
            ////        && (TryRemoveTrackedObject(entityInfo, key) ||
            ////            (entityKey != null && TryRemoveTrackedObject(entityInfo, entityKey))))
            ////    {
            ////        ReplaceItem(entityInfo, source, null);
            ////    }
            ////    result = source;
            ////}
            ////else
            ////{
            // Hier muss der Primärschlüssel von source zwingend belegt sein
            if (!entity.IsPrimaryKeySet(source))
            {
                throw new ArgumentException("primary key must be set if ChangeTracking state is not Added", nameof(source));
            }

            //// temporäre Id durch Primärschlüssel ersetzen
            ////if (source.ChangeTracker.TemporaryObjectId.HasValue)
            ////{
            ////    var tracked = GetTrackedObjectOrDefault(entityInfo, source.ChangeTracker.TemporaryObjectId.Value);
            ////    if (tracked != null)
            ////    {
            ////        if (tracked != source)
            ////        {
            ////            // Primärschlüssel kopieren
            ////            foreach (var prop in entityInfo.ScalarProperties.Where(p => p.IsPrimaryKey))
            ////            {
            ////                prop.SetValue(tracked, prop.GetValue(source));
            ////            }
            ////        }

            ////        // Objekt mit temporärer Id aus TrackedObjects entfernen
            ////        //if (entityInfo.RootType.IsPrimaryKeySet(tracked) && tracked.ChangeTracker.TemporaryObjectId.HasValue)
            ////        //{
            ////        //    //TryRemoveTrackedObject(entityInfo, tracked.ChangeTracker.TemporaryObjectId.Value);
            ////        //    //tracked.ChangeTracker.TemporaryObjectId = null;
            ////        //    //tracked.MarkAsUnchanged();
            ////        //}

            ////        // Objekt mit Primärschlüssel in TrackedObjects einfügen
            ////        var entityKey = entityInfo.RootType.GetPrimaryKeyObject(tracked);
            ////        result = AddTrackedObject(entityInfo, entityKey, tracked);
            ////        if (result != tracked)
            ////        {
            ////            // in allen getrackten Objekten tracked durch result ersetzen
            ////            ReplaceItem(entityInfo, tracked, result);
            ////        }
            ////    }
            ////}

            if (result == null)
            {
                // keine temporäre Id oder das Objekt ist mit temporäer Id noch nicht in TrackedObjects
                // Objekt mit Primärschlüssel in TrackedObjects einfügen
                var entityKey = entityInfo.RootType.GetPrimaryKeyObject(source);
                result = AddTrackedObject(entityInfo, entityKey, source, out added);
            }

            ////}

            if (result == null)
            {
                // zu diesem Zeitpunkt muss ein Ergbenis feststehen und in TrackedObjects vorhanden sein
                throw new InvalidOperationException("no result object!");
            }

            CleanupTuple tuple = null;

            if (source != result)
            {
                tuple = new CleanupTuple(source, result, entityInfo);

                // Objekte nicht Referenzgleich
                if (_trackingInfoProvider.GetState(source) == TrackingState.Added && _trackingInfoProvider.GetState(result) == TrackingState.Deleted)
                {
                    MergeScalarProperties(tuple, fixups, MergeStrategy.ForceUpdate, true);
                }
                else
                {
                    MergeScalarProperties(tuple, fixups, mergeStrategy);
                }

                // add to cleaned Tuples to avoid endless recoursions.
                cleanedTuples.Add(source, tuple);
            }
            else
            {
                tuple = new CleanupTuple(source, result, entityInfo, added);

                // add to cleaned Tuples to avoid endless recoursions.
                cleanedTuples.Add(result, tuple);
            }

            cleanedItem = tuple;
            return true;
        }

        private Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<object>> CreateReverseFixupDelegate(NavigationPropertyMetadata prop)
        {
            var reverseEntityInfo = GetEntityInfo(prop.Entity);

            ParameterExpression trackedParameter = Expression.Parameter(typeof(IEnumerable<object>));
            ParameterExpression entitiesParameter = Expression.Parameter(typeof(IEnumerable<object>));

            Expression baseQuery = Expression.Call(
                typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(reverseEntityInfo.EntityMetadata.ClrType),
                trackedParameter);

            ParameterExpression param = Expression.Parameter(reverseEntityInfo.EntityMetadata.ClrType);
            ParameterExpression param2 = Expression.Parameter(prop.TargetEntity.ClrType);

            Expression filterCondition = Expression.Equal(Expression.Property(param, prop.Name), Expression.Constant(null));

            filterCondition = Expression.AndAlso(
                filterCondition,
                Expression.NotEqual(
                    Expression.Constant(TrackingState.Deleted, typeof(TrackingState?)),
                    Expression.Call(Expression.Constant(_trackingInfoProvider, typeof(ITrackingInfoProvider)), typeof(ITrackingInfoProvider).GetMethod("GetState"), param)));

            baseQuery = Expression.Call(
                typeof(Enumerable).GetMethods().First(p => p.Name == "Where").MakeGenericMethod(reverseEntityInfo.EntityMetadata.ClrType),
                baseQuery,
                Expression.Lambda(filterCondition, param));

            Expression joinPrimary;
            Expression joinForeign;
            if (prop.Multiplicity != NavigationPropertyMultiplicity.Many && prop.TargetMultiplicity != NavigationPropertyMultiplicity.Many)
            {
                if (prop.Entity.PrimaryKeyCount == 1)
                {
                    joinPrimary = Expression.Property(param2, prop.TargetEntity.GetProperties().First(p => p.IsPrimaryKey).Name);
                    joinForeign = Expression.Property(param, prop.Entity.GetProperties().First(p => p.IsPrimaryKey).Name);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (prop.ForeignKeyProperties.Count == 1)
                {
                    joinPrimary = Expression.Property(param2, prop.ForeignKeyProperties.First().Principal.Name);
                    joinForeign = Expression.Property(param, prop.ForeignKeyProperties.First().Dependant.Name);
                }
                else
                {
                    // Dieser Fall existiert momentan im Datenmodel noch nicht. Betrifft zusammengesetzte ForeignKeys.
                    throw new NotImplementedException();
                }
            }

            if (joinPrimary.Type != joinForeign.Type)
            {
                joinPrimary = Expression.Convert(joinPrimary, joinForeign.Type);
            }

            var block = new List<Expression>
            {
                Expression.Assign(Expression.Property(param, prop.Name), param2)
            };

            if (prop.TargetNavigationProperty != null)
            {
                if (prop.TargetMultiplicity == NavigationPropertyMultiplicity.Many)
                {
                    var collectionType = typeof(ICollection<>).MakeGenericType(prop.Entity.ClrType);
                    block.Add(Expression.Call(Expression.Property(param2, prop.TargetNavigationProperty.Name), collectionType.GetMethod("Add"), param));
                }
                else
                {
                    block.Add(Expression.Assign(Expression.Property(param2, prop.TargetNavigationProperty.Name), param));
                }
            }

            block.Add(param);

            baseQuery = Expression.Call(
                _enumerableJoinMethod.MakeGenericMethod(param.Type, param2.Type, joinForeign.Type, param.Type),
                baseQuery,
                Expression.Call(typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(param2.Type), entitiesParameter),
                Expression.Lambda(joinForeign, param),
                Expression.Lambda(joinPrimary, param2),
                Expression.Lambda(
                    Expression.Block(
                        block),
                    param,
                    param2));

            var lambda = Expression.Lambda<Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<object>>>(
                    baseQuery, trackedParameter, entitiesParameter);

            return lambda.Compile();
        }

        private IEnumerable<T> DoClean<T>(IEnumerable<T> items, MergeStrategy mergeStrategy, out Dictionary<object, CleanupTuple> totalCleaned)
            where T : class
        {
            // Liste von Objekten mit Naviagtion-Property, für die ein Fixup gemacht werden muss
            var fixups = new List<Tuple<object, NavigationPropertyMetadata>>();

            // Liste der Objekte, für die ein reverse Fixup gemacht werden muss
            var reverseFixups = new Dictionary<EntityInfo, HashSet<object>>();

            var toClean = items.Where(p => p != null).Cast<object>().ToList();

            List<T> result = null;

            var parentInfos = new Dictionary<CleanupTuple, List<ParentInfo>>();
            totalCleaned = new Dictionary<object, CleanupTuple>();

            while (toClean.Any())
            {
                var addToResult = result == null;
                result = result ?? new List<T>();

                // Liste der Objekte, die ausgetauscht wurden
                var cleanupTuples = new Dictionary<object, CleanupTuple>();

                foreach (var item in toClean)
                {
                    if (Clean(item, cleanupTuples, fixups, mergeStrategy, out var cleanedItem))
                    {
                        totalCleaned.Add(item, cleanedItem);
                    }

                    if (addToResult)
                    {
                        result.Add((T)cleanedItem.Target);
                    }
                }

                foreach (var parent in parentInfos)
                {
                    if (parent.Key.Added)
                    {
                        foreach (var nav in parent.Value)
                        {
                            var childTuple = totalCleaned[nav.Child];
                            if (childTuple.Source != childTuple.Target)
                            {
                                nav.Nav.ReplaceItem(parent.Key.Source, childTuple.Source, childTuple.Target);
                            }
                        }
                    }

                    // Objekte sind nicht Referenzgleich
                    if (parent.Key.Source != parent.Key.Target && parent.Key.Added)
                    {
                        // bei einem neu hinzugefügten Objekt muss source und result Referenzgleich sein
                        throw new InvalidOperationException("added object must be same object!");
                    }

                    // Clean aller Navigation Properties
                    if (!parent.Key.Added)
                    {
                        foreach (var nav in parent.Value)
                        {
                            var tmpInfo = GetEntityInfo(nav.Nav.TargetEntity);

                            var childTuple = totalCleaned[nav.Child];

                            if (childTuple.Source != childTuple.Target)
                            {
                                if (parent.Key.Source == parent.Key.Target)
                                {
                                    ReplaceItem(tmpInfo, childTuple.Source, childTuple.Target);
                                }
                            }
                        }
                    }
                }

                parentInfos = cleanupTuples.ToDictionary(
                    p => p.Value,
                    p => p.Value.EntityInfo.NavigationProperties.SelectMany(x => x.GetItems(p.Key).Select(y => new ParentInfo { Child = y, Nav = x })).ToList());

                toClean = parentInfos.SelectMany(p => p.Value).Select(p => p.Child).Distinct().Except(totalCleaned.Keys).ToList();
            }

            foreach (var item in totalCleaned)
            {
                if (item.Value.Added)
                {
                    // Navigation Properties über ForeignKey befüllen
                    foreach (var navProp in item.Value.EntityInfo.NavigationProperties.Where(p => p.Multiplicity != NavigationPropertyMultiplicity.Many))
                    {
                        if (navProp.GetValue(item.Value.Target) == null)
                        {
                            FixupNavigationProperty(item.Value.Target, navProp);
                        }
                    }

                    var entityInfo = item.Value.EntityInfo;
                    if (entityInfo.EntityMetadata.IsPrimaryKeySet(item.Value.Target)) ////|| result.ChangeTracker.State != ObjectState.Added
                    {
                        if (!reverseFixups.ContainsKey(entityInfo))
                        {
                            reverseFixups.Add(entityInfo, new HashSet<object>());
                        }

                        reverseFixups[entityInfo].Add(item.Value.Target);
                    }
                }
            }

            // Fixups parallel ausführen
            Parallel.ForEach(fixups, p => FixupNavigationProperty(p.Item1, p.Item2));

            // Reverse-Fixup parallel ausführen
            Parallel.ForEach(reverseFixups, p => ReverseNavigationPropertyFixup(p.Key, p.Value));

            return result;
        }

        private void EndAttach(AttachTransaction transaction)
        {
            lock (_attachTransactionsLocker)
            {
                if (this._attachTransactions.Remove(transaction))
                {
                    if (this._attachTransactions.Count == 0)
                    {
                        this._attachTransactionsSignal.Set();
                    }
                }
            }
        }

        private void FixupNavigationProperty(object result, NavigationPropertyMetadata navProp)
        {
            var targetEntityInfo = GetEntityInfo(navProp.TargetEntity);
            var key = navProp.GetForeignKeyObject(result);
            if (key != null)
            {
                var target = GetTrackedObjectOrDefault(targetEntityInfo, key);
                if (target != null && _trackingInfoProvider.GetState(target) != TrackingState.Deleted)
                {
                    FillNavigationPropertyAndPrincipal(navProp, result, target);
                }
            }
        }

        private IEnumerable<object> GetChangesByState(IEnumerable<EntityMetadata> entities, Dictionary<EntityMetadata, List<object>> changes, TrackingState state)
        {
            foreach (var item in entities)
            {
                List<object> values;
                if (changes.TryGetValue(item, out values))
                {
                    foreach (var row in values.Where(p => _trackingInfoProvider.GetState(p) == state))
                    {
                        yield return row;
                    }
                }
            }
        }

        private EntityInfo GetEntityInfo(EntityMetadata entity)
        {
            EntityInfo data;
            lock (EntityInfoLocker)
            {
                if (!EntityInfos.TryGetValue(entity, out data))
                {
                    data = new EntityInfo(_model, entity);
                    EntityInfos.Add(entity, data);
                }
            }

            return data;
        }

        private object GetTrackedObjectOrDefault(EntityInfo entityInfo, object key)
        {
            lock (_updateLocker)
            {
                Dictionary<object, System.WeakReference<object>> first;
                if (!_trackedObjects.TryGetValue(entityInfo.RootType, out first))
                {
                    first = new Dictionary<object, System.WeakReference<object>>();
                    _trackedObjects.Add(entityInfo.RootType, first);
                }

                System.WeakReference<object> result;
                if (first.TryGetValue(key, out result))
                {
                    return result.TargetOrDefault();
                }
            }

            return null;
        }

        private IEnumerable<object> GetTrackedObjectsForEntity(EntityInfo entityInfo)
        {
            lock (_updateLocker)
            {
                Dictionary<object, System.WeakReference<object>> first;
                if (!_trackedObjects.TryGetValue(entityInfo.RootType, out first))
                {
                    first = new Dictionary<object, System.WeakReference<object>>();
                    _trackedObjects.Add(entityInfo.RootType, first);
                }

                return first.Values.Select(p => p.TargetOrDefault()).Where(p => p != null).ToList();
            }
        }

        private void MergeScalarProperties(
            CleanupTuple tuple,
            List<Tuple<object, NavigationPropertyMetadata>> fixups,
            MergeStrategy mergeStrategy,
            bool forceFixup = false)
        {
            // geänderte Objekte nur bei ForceUpdate aktualisieren
            if (mergeStrategy == MergeStrategy.ForceUpdate || _trackingInfoProvider.GetState(tuple.Target) == TrackingState.Unchanged)
            {
                ////using (new ChangeTrackingScope(ChangeTracking.Disable, source, target))
                ////{
                var entity = GetEntityInfo(_model.GetEntityMetadata(tuple.Target));

                // Properties kopieren
                foreach (var prop in entity.ScalarProperties)
                {
                    var oldValue = prop.GetValue(tuple.Target);
                    var newValue = prop.GetValue(tuple.Source);

                    if (!Equals(oldValue, newValue) || forceFixup)
                    {
                        tuple.TrackProperty(prop);
                        prop.SetValue(tuple.Target, newValue);

                        // ist dieses Skalar Property Schlüssel für ein Navigation Property?
                        NavigationPropertyMetadata navProp;
                        if (entity.ForeignKeyProperties.TryGetValue(prop, out navProp))
                        {
                            fixups.Add(Tuple.Create(tuple.Target, navProp));
                        }
                    }
                }

                //// weitere Properties kopieren
                ////var cloneableSource = source as IHasExtendedProperties;
                ////if (cloneableSource != null)
                ////{
                ////    cloneableSource.CopyExtendedProperties(target);
                ////}

                if (mergeStrategy == MergeStrategy.ForceUpdate)
                {
                    switch (_trackingInfoProvider.GetState(tuple.Source).GetValueOrDefault())
                    {
                        case TrackingState.Unchanged:
                            _trackingInfoProvider.SetState(tuple.Target, TrackingState.Unchanged);
                            break;

                        case TrackingState.Added:
                            _trackingInfoProvider.SetState(tuple.Target, TrackingState.Added);
                            break;

                        case TrackingState.Modified:
                            _trackingInfoProvider.SetState(tuple.Target, TrackingState.Modified);
                            break;

                        case TrackingState.Deleted:
                            _trackingInfoProvider.SetState(tuple.Target, TrackingState.Deleted);
                            break;
                    }
                }
            }
        }

        private void ReplaceItem(EntityInfo entityInfo, object source, object target)
        {
            ReplaceItems(entityInfo, new Dictionary<object, object> { { source, target } });
        }

        private void ReplaceItems(EntityInfo entityInfo, IDictionary<object, object> replaces)
        {
            foreach (var navProp in entityInfo.ReverseNavigationProperties)
            {
                var reverseEntityInfo = GetEntityInfo(navProp.Entity);

                var joined = from o in GetTrackedObjectsForEntity(reverseEntityInfo).Where(p => reverseEntityInfo.EntityMetadata.IsOfType(p))
                             from i in navProp.GetItems(o)
                             join r in replaces on i equals r.Key
                             select new { Object = o, Source = r.Key, Target = r.Value };

                foreach (var item in joined.ToList())
                {
                    navProp.ReplaceItem(item.Object, item.Source, item.Target);
                }
            }
        }

        private void ReverseNavigationPropertyFixup(EntityInfo entityInfo, HashSet<object> entities)
        {
            ////using (var scope = new ChangeTrackingScope(ChangeTracking.Disable, entities.ToArray()))
            ////{
            var reverseNavigationProperties = entityInfo.ReverseNavigationProperties.Where(p => p.Multiplicity != NavigationPropertyMultiplicity.Many).ToList();

            foreach (var prop in reverseNavigationProperties)
            {
                var reverseEntityInfo = GetEntityInfo(prop.Entity);
                var tracked = GetTrackedObjectsForEntity(reverseEntityInfo).ToList();
                foreach (var item in tracked)
                {
                    ////scope.Register(item);
                }

                var compiled = ReverseFixupCache.GetOrAdd(prop, CreateReverseFixupDelegate);
                compiled(tracked, entities).ToList();
            }

            ////}
        }

        private bool TryRemoveTrackedObject(EntityInfo entityInfo, object key)
        {
            lock (_updateLocker)
            {
                Dictionary<object, System.WeakReference<object>> first;
                if (!_trackedObjects.TryGetValue(entityInfo.RootType, out first))
                {
                    // EntityInfo noch überhaupt nicht in TrackedObjects
                    first = new Dictionary<object, System.WeakReference<object>>();
                    _trackedObjects.Add(entityInfo.RootType, first);
                    return false;
                }

                System.WeakReference<object> data;

                // Objekte versuchen zu entfernen
                if (first.TryGetValue(key, out data))
                {
                    first.Remove(key);
                    object value;
                    if (data.TryGetTarget(out value))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private IEnumerable<T> TryRemoveTrackedObjects<T>(EntityInfo entityInfo, IEnumerable<object> keys)
        {
            var result = new List<T>();

            lock (_updateLocker)
            {
                Dictionary<object, System.WeakReference<object>> first;
                if (!_trackedObjects.TryGetValue(entityInfo.RootType, out first))
                {
                    // EntityInfo noch überhaupt nicht in TrackedObjects
                    first = new Dictionary<object, System.WeakReference<object>>();
                    _trackedObjects.Add(entityInfo.RootType, first);
                    return result;
                }

                foreach (var key in keys)
                {
                    System.WeakReference<object> data;

                    // Objekte versuchen zu entfernen
                    if (first.TryGetValue(key, out data))
                    {
                        first.Remove(key);
                        object value;
                        if (data.TryGetTarget(out value))
                        {
                            result.Add((T)value);
                        }
                    }
                }

                return result;
            }
        }

        private void WeakReferenceCleanupTimer(object state)
        {
            int oldCount;
            int newCount;
            lock (_updateLocker)
            {
                oldCount = _trackedObjects.SelectMany(p => p.Value).Count();

                var data = (from tracked in _trackedObjects
                            from keys in tracked.Value
                            where !keys.Value.IsAlive()
                            select new { Dictonary = tracked.Value, keys.Key }).ToList();

                foreach (var item in data)
                {
                    item.Dictonary.Remove(item.Key);
                }

                newCount = _trackedObjects.SelectMany(p => p.Value).Count();
            }
        }

        private class AttachTransaction : IDisposable
        {
            private bool _disposed;

            public AttachTransaction(ModelContext context)
            {
                this.Context = context;
            }

            ~AttachTransaction()
            {
                Dispose(false);
            }

            public ModelContext Context { get; }

            protected bool IsDisposed
            {
                get
                {
                    return _disposed;
                }
            }

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        // Managed Resourcen aufräumen
                        this.Context.EndAttach(this);
                    }

                    // Unmanaged Resourcen aufräumen
                    _disposed = true;
                }
            }
        }

        private class CleanupTuple
        {
            private List<ScalarProperty> _properties;

            public CleanupTuple(object source, object target, EntityInfo entityInfo, bool added = false)
            {
                Added = added;
                EntityInfo = entityInfo;
                Target = target;
                Source = source;
            }

            public bool Added { get; }

            public EntityInfo EntityInfo { get; }

            public object Source { get; }

            public object Target { get; }

            public IEnumerable<ScalarProperty> GetProperties()
            {
                return _properties ?? Enumerable.Empty<ScalarProperty>();
            }

            public void TrackProperty(ScalarProperty property)
            {
                _properties = _properties ?? new List<ScalarProperty>();
                _properties.Add(property);
            }
        }

        private class ParentInfo
        {
            public object Child { get; set; }

            public NavigationPropertyMetadata Nav { get; set; }

            public CleanupTuple Tuple { get; set; }
        }
    }
}