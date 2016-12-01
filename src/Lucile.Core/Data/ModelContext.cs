using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Lucile.Data.Metadata;

namespace Lucile.Data
{
    public class ModelContext : IDisposable
    {
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

        private readonly object _updateLocker = new object();

        private bool _disposedValue = false;

        static ModelContext()
        {
            ReverseFixupCache = new ConcurrentDictionary<NavigationPropertyMetadata, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<object>>>();
        }

        public ModelContext(MetadataModel model)
        {
            _model = model;
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
            var itemsList = items.Where(p => p != null).ToList();

            using (BeginAttach<T>())
            {
                // Liste der bereinigten Objekte für Rekursionsvermeidung
                var cleaned = new Dictionary<object, bool>();

                // Liste der Objekte, die ausgetauscht wurden
                var cleanupTuples = new Dictionary<object, object>();

                // Liste von Objekten mit Naviagtion-Property, für die ein Fixup gemacht werden muss
                var fixups = new List<Tuple<object, NavigationPropertyMetadata>>();

                // Liste der Objekte, für die ein reverse Fixup gemacht werden muss
                var reverseFixups = new Dictionary<EntityInfo, HashSet<object>>();

                // Objekte aufräumen
                var result = new List<T>();
                result.AddRange(itemsList.Select(item => (T)Clean(item, cleaned, cleanupTuples, fixups, reverseFixups, mergeStrategy)));

                // Fixups parallel ausführen
                Parallel.ForEach(fixups, p => FixupNavigationProperty(p.Item1, p.Item2));

                // Reverse-Fixup parallel ausführen
                Parallel.ForEach(reverseFixups, p => ReverseNavigationPropertyFixup(p.Key, p.Value));

                return result;
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

        public T DetachSingle<T>(T item)
            where T : class
        {
            using (BeginAttach<T>())
            {
                var entity = _model.GetEntityMetadata(item);

                var entityInfo = GetEntityInfo(entity);

                ReplaceItem(entityInfo, item, null);

                if (TryRemoveTrackedObject(entityInfo, entity.GetPrimaryKeyObject(item)))
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }
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

        /// <summary>
        /// Navigation Property belegen und Back-Referenz befüllen (Collection oder einzelnes Navigation Property)
        /// </summary>
        /// <param name="navProp"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
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

        /// <summary>
        /// Objekt in TrackedObjects einfügen
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private object AddTrackedObject(EntityInfo entityInfo, object key, object data)
        {
            bool added;
            return AddTrackedObject(entityInfo, key, data, out added);
        }

        /// <summary>
        /// Objekt in TrackedObjects einfügen
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="added"></param>
        /// <returns></returns>
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

        private object Clean(
            object source,
                Dictionary<object, bool> cleaned,
                Dictionary<object, object> cleanedTuples,
                List<Tuple<object, NavigationPropertyMetadata>> fixups,
                Dictionary<EntityInfo, HashSet<object>> reverseFixups,
                MergeStrategy mergeStrategy)
        {
            // Wurde dieses Objekt bereits gecleaned und in die TrackedObjects eingefügt
            if (cleaned.ContainsKey(source))
            {
                return source;
            }

            // Wurde dieses Objekt bereits gecleaned und als Duplikat erkannt
            object tupleItem;
            if (cleanedTuples.TryGetValue(source, out tupleItem))
            {
                return tupleItem;
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

            if (source != result)
            {
                // Objekte nicht Referenzgleich

                ////if (source.ChangeTracker.State == ObjectState.Added && result.ChangeTracker.State == ObjectState.Deleted)
                ////{
                ////    MergeScalarProperties(source, result, fixups, MergeStrategy.ForceUpdate, true);
                ////}
                ////else
                ////{
                MergeScalarProperties(source, result, fixups, mergeStrategy);
                ////}

                // zu CleanedTuples hinzufügen
                cleanedTuples.Add(source, result);
            }
            else
            {
                // zur Rekusrionsvermeidung in Liste eintragen
                cleaned.Add(result, added);
            }

            // wurde das Objekt bei diesem Durchlauf in die TrackedObjects eingefügt
            if (added)
            {
                // Rekursives Cleanup der Navigation Properties
                foreach (var navProp in entityInfo.NavigationProperties)
                {
                    foreach (var child in navProp.GetItems(result).OfType<object>().ToList())
                    {
                        // Clean für child
                        var cleanedChild = Clean(child, cleaned, cleanedTuples, fixups, reverseFixups, mergeStrategy);

                        if (cleanedChild != child)
                        {
                            // child durch cleanedChild ersetzen
                            ////using (new ChangeTrackingScope(ChangeTracking.Disable, cleanedChild, result))
                            ////{
                            navProp.ReplaceItem(result, child, cleanedChild);
                            ////}
                        }
                    }
                }

                // Navigation Properties über ForeignKey befüllen
                foreach (var navProp in entityInfo.NavigationProperties.Where(p => p.Multiplicity != NavigationPropertyMultiplicity.Many))
                {
                    if (navProp.GetValue(result) == null)
                    {
                        FixupNavigationProperty(result, navProp);
                    }
                }

                if (entity.IsPrimaryKeySet(result)) ////|| result.ChangeTracker.State != ObjectState.Added
                {
                    if (!reverseFixups.ContainsKey(entityInfo))
                    {
                        reverseFixups.Add(entityInfo, new HashSet<object>());
                    }

                    reverseFixups[entityInfo].Add(result);
                }
            }

            // Objekte sind nicht Referenzgleich
            if (source != result && added)
            {
                // bei einem neu hinzugefügten Objekt muss source und result Referenzgleich sein
                throw new InvalidOperationException("added object must be same object!");
            }

            // Clean aller Navigation Properties
            if (!added)
            {
                ////var sourceUnloaded = source.GetUnloadedNavigationProperties().ToList();
                foreach (var navProp in entityInfo.NavigationProperties)
                {
                    ////var isLoaded = !sourceUnloaded.Contains(navProp.Name);
                    var sourceItems = navProp.GetItems(source).OfType<object>().ToList();

                    ////if (isLoaded)
                    ////{
                    ////    result.MarkNavigationPropertyAsLoaded(navProp.Name);
                    ////}

                    var tmpInfo = GetEntityInfo(navProp.TargetEntity);

                    foreach (var child in sourceItems)
                    {
                        var newChild = Clean(child, cleaned, cleanedTuples, fixups, reverseFixups, mergeStrategy);
                        if (newChild != child)
                        {
                            if (source == result)
                            {
                                ReplaceItem(tmpInfo, child, newChild);
                            }
                        }
                    }

                    ////if (isLoaded)
                    ////{
                    ////    var missing = tmpInfo.GetMissingSourceObjects(navProp.GetItems(result), sourceItems)
                    ////        //.Where(p => p.ChangeTracker.State != ObjectState.Added)
                    ////        .ToList();

                    ////    foreach (var item in missing)
                    ////    {
                    ////        this.DetachSingle<object>((object)item);
                    ////        // what shall wo do... with the drunken sailor
                    ////    }
                    ////}
                }
            }

            return result;
        }

        /// <summary>
        /// Delegate für das ReverseFixup erzeugen
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
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

            ////filterCondition = Expression.AndAlso(
            ////    filterCondition,
            ////    Expression.NotEqual(Expression.Property(Expression.Property(param, "ChangeTracker"), "State"), Expression.Constant(ObjectState.Deleted))
            ////        );

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
                typeof(Enumerable).GetMethods().First(p => p.Name == "Join").MakeGenericMethod(param.Type, param2.Type, joinForeign.Type, param.Type),
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

        /// <summary>
        /// Navigation Properties über ForeignKey befüllen
        /// </summary>
        /// <param name="result"></param>
        /// <param name="navProp"></param>
        private void FixupNavigationProperty(object result, NavigationPropertyMetadata navProp)
        {
            var targetEntityInfo = GetEntityInfo(navProp.TargetEntity);
            var key = navProp.GetForeignKeyObject(result);
            if (key != null)
            {
                var target = GetTrackedObjectOrDefault(targetEntityInfo, key);
                if (target != null) ////&& target.ChangeTracker.State != ObjectState.Deleted)
                {
                    FillNavigationPropertyAndPrincipal(navProp, result, target);
                }
            }
        }

        /// <summary>
        /// EntityInfo zu EntityMetadata ermitteln
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Objekt über Primärschlüssel in TrackedObjects suchen
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Alle Objekte aus TrackedObjects für EntityInfo
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Skalarproperties kopieren
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="fixups"></param>
        /// <param name="mergeStrategy"></param>
        /// <param name="forceFixup">Fixup wird erzwungen, wenn true.</param>
        private void MergeScalarProperties(
            object source,
            object target,
            List<Tuple<object, NavigationPropertyMetadata>> fixups,
            MergeStrategy mergeStrategy,
            bool forceFixup = false)
        {
            // geänderte Objekte nur bei ForceUpdate aktualisieren
            if (mergeStrategy == MergeStrategy.ForceUpdate) ////|| target.ChangeTracker.State == ObjectState.Unchanged
            {
                ////using (new ChangeTrackingScope(ChangeTracking.Disable, source, target))
                ////{
                var entity = GetEntityInfo(_model.GetEntityMetadata(target));

                // Properties kopieren
                foreach (var prop in entity.ScalarProperties)
                {
                    var oldValue = prop.GetValue(target);
                    var newValue = prop.GetValue(source);

                    if (!Equals(oldValue, newValue) || forceFixup)
                    {
                        prop.SetValue(target, newValue);

                        // ist dieses Skalar Property Schlüssel für ein Navigation Property?
                        NavigationPropertyMetadata navProp;
                        if (entity.ForeignKeyProperties.TryGetValue(prop, out navProp))
                        {
                            fixups.Add(Tuple.Create(target, navProp));
                        }
                    }
                }

                //// weitere Properties kopieren
                ////var cloneableSource = source as IHasExtendedProperties;
                ////if (cloneableSource != null)
                ////{
                ////    cloneableSource.CopyExtendedProperties(target);
                ////}

                ////    if (mergeStrategy == MergeStrategy.ForceUpdate)
                ////    {
                ////        // Status bei ForceUpdate umsetzen
                ////        target.ResetTracking();
                ////        switch (source.ChangeTracker.State)
                ////        {
                ////            case ObjectState.Added:
                ////                target.MarkAsAdded();
                ////                break;

                ////            case ObjectState.Modified:
                ////                target.MarkAsModified();
                ////                break;

                ////            case ObjectState.Deleted:
                ////                target.MarkAsDeleted();
                ////                break;
                ////        }
                ////    }
                ////}
            }
        }

        /// <summary>
        /// In allen TrackedObjects source durch target ersetzen
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void ReplaceItem(EntityInfo entityInfo, object source, object target)
        {
            foreach (var navProp in entityInfo.ReverseNavigationProperties)
            {
                var reverseEntityInfo = GetEntityInfo(navProp.Entity);
                foreach (var reverseEntity in GetTrackedObjectsForEntity(reverseEntityInfo))
                {
                    if (reverseEntityInfo.EntityMetadata.IsOfType(reverseEntity))
                    {
                        ////using (new ChangeTrackingScope(ChangeTracking.Disable, target, reverseEntity))
                        ////{
                        navProp.ReplaceItem(reverseEntity, source, target);
                        ////}
                    }
                }
            }
        }

        /// <summary>
        /// Reverse-Fixup der Navigation-Properties
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="entities"></param>
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

        /// <summary>
        /// Objekt aus TrackedObjects entfernen
        /// </summary>
        /// <param name="entityInfo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Timer um nicht mehr benötigte Objekte aus der Collection zu entfernen
        /// </summary>
        /// <param name="state"></param>
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

            /// <summary>
            /// Finalizer
            /// </summary>
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

        // To detect redundant calls
    }
}