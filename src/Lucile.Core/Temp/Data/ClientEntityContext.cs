using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using Codeworx.Data.Metadata;
using System.Threading;
using Codeworx.Data.Entity;

namespace Codeworx.Data
{
    public class ClientEntityContext
    {
        private readonly Collection<WeakReference> _trackedObjects;

        private object contextLocker = new object();

        private Timer timer;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClientEntityContext(SynchronizationContext synchronizationContext, MetadataModel model)
        {
            this.Model = model;

            this.SynchronizationContext = synchronizationContext;
            if (this.SynchronizationContext != null) {
                this.SynchronizationContext.Send(new SendOrPostCallback(p => this.syncThread = Thread.CurrentThread), null);
            }

            _trackedObjects = new Collection<WeakReference>();

            timer = new Timer(new TimerCallback(WeakReferenceCleanupTimerTick), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public ClientEntityContext(MetadataModel model) : this(null, model)
        {

        }

        Thread syncThread;

        private bool CheckAccess() {
            if (this.SynchronizationContext != null) {
                return this.syncThread == Thread.CurrentThread;
            }
            return true;
        }

        private void WeakReferenceCleanupTimerTick(object parameter)
        {
            if (!CheckAccess()) {
                this.SynchronizationContext.Post(new SendOrPostCallback(p => WeakReferenceCleanupTimerTick(p)), parameter);
                return;
            }

            var items = this._trackedObjects.Where(p => !p.IsAlive).ToArray();
            if (items.Length > 0)
            {
                foreach (var item in items)
                {
                    this._trackedObjects.Remove(item);
                }
            }
        }

        public SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// Liefert oder setzt das Entity-Modell
        /// </summary>
        public MetadataModel Model { get; private set; }

        /// <summary>
        /// Liefert die getrackten Objekte
        /// </summary>
        public IEnumerable<object> TrackedObjects { get { return this._trackedObjects.Where(p => p.IsAlive).Select(p => p.Target); } }

        /// <summary>
        /// Fügt die Quellmenge dem Client-Context hinzu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public IEnumerable<T> Attach<T>(IEnumerable<T> items)
        {
            if (!CheckAccess())
            {
                IEnumerable<T> newItems = null;
                this.SynchronizationContext.Send(new SendOrPostCallback(p => newItems = Attach(items)), null);
                return newItems;
            }

            Dictionary<object, object> tuples = new Dictionary<object, object>();
            List<object> cleanedItems = new List<object>(TrackedObjects);

            var result = items.Select(item => (T)CleanUp(item, cleanedItems, tuples)).ToList();
            //result.OfType<IObjectWithChangeTracker>().ToList().ForEach(p => p.AcceptChangesRecursive());
            return result;
        }

        /// <summary>
        /// Fügt die Quelle dem Client-Context hinzu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public T AttachSingle<T>(T item)
        {
            if (!CheckAccess())
            {
                T newItem = default(T);
                this.SynchronizationContext.Send(new SendOrPostCallback(p => newItem = AttachSingle(item)), null);
                return newItem;
            }

            //lock (contextLocker)
            //{
            //    if (_trackedObjects.Where(to => to.GetType() == typeof(T)).Any(to => Model.GetEntityMetadata(to).KeyEquals(to, item)))
            //    {
            //        var entity = Model.GetEntityMetadata(item);

            //        var sources = entity.FlattenChildren(item).Where(p => p.Value.Entity == entity).ToList();
            //        sources.Add(new KeyValuePair<object, NavigationPropertyMetadata>(item, null));

            //        foreach (var keyValuePair in sources)
            //        {
            //            var source = keyValuePair.Key;

            //            var sourceEntity = Model.GetEntityMetadata(source);
            //            var destination = _trackedObjects.FirstOrDefault(to => to.GetType() == source.GetType() &&
            //                                                                   sourceEntity.KeyEquals(to, source));

            //            var changeTrackerItem = source as IObjectWithChangeTracker;
            //            if (changeTrackerItem != null)
            //            {
            //                changeTrackerItem.StopTracking();
            //            }

            //            // Properties kopieren, wenn Objekt bereits im Context enthalten ist
            //            UpdateScalarProperties(source, destination);

            //            if (changeTrackerItem != null)
            //            {
            //                changeTrackerItem.StartTracking();
            //            }
            //        }

            //        var itemToUpdate = _trackedObjects.FirstOrDefault(to => to.GetType() == typeof(T) &&
            //                                                                entity.KeyEquals(to, item));

            //        return (T)itemToUpdate;
            //    }
            //    else
            //    {
            var result = (T)CleanUp(item, new List<object>(TrackedObjects), null);
            var ioct = result as IObjectWithChangeTracker;
            //if (ioct != null)
            //    ioct.AcceptChangesRecursive();

            return result;
            //}
            //}
        }

        /// <summary>
        /// Entfernt ein Item vom Client-Context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public T DetachSingle<T>(T item)
        {
            var itemToDetach = _trackedObjects.FirstOrDefault(to => to.GetType() == typeof(T) &&
                                                                    Model.GetEntityMetadata(to.Target).KeyEquals(to.Target, item));

            _trackedObjects.Remove(itemToDetach);

            return (T)itemToDetach.Target;
        }

        /// <summary>
        /// Leert den Clint-Context
        /// </summary>
        public void Clear()
        {
            _trackedObjects.Clear();
        }

        private object CleanUp(object item, ICollection<object> cleanedItems = null, IDictionary<object, object> tuples = null)
        {
            if (item == null)
                return null;

            bool newItem = false;

            if (cleanedItems == null)
            {
                cleanedItems = new Collection<object>() { TrackedObjects };
            }

            if (cleanedItems.Contains(item))
            {
                return item;
            }

            if (tuples == null)
            {
                tuples = new Dictionary<object, object>();
            }
            else if (tuples.ContainsKey(item))
            {
                return tuples[item];
            }

            var entity = Model.GetEntityMetadata(item);
            object result;
            var objects = this.TrackedObjects.Where(p => entity.IsOfType(p) && entity.KeyEquals(p, item) && !Object.ReferenceEquals(p, item)).ToArray();
            if (objects.Any())
            {
                result = objects.First();
                tuples.Add(item, result);
            }
            else
            {
                this._trackedObjects.Add(new WeakReference(item));
                newItem = true;
                result = item;
            }
            cleanedItems.Add(result);

            var changeTrackerItem = result as IObjectWithChangeTracker;
            if (changeTrackerItem != null)
            {
                changeTrackerItem.StopTracking();
            }

            var children = entity.FlattenChildren(item, 1).Where(p => p.Value.Entity == entity).ToArray();

            bool mergeNavigationProperties = result != item;

            foreach (var child in children)
            {
                var newChild = CleanUp(child.Key, cleanedItems, tuples);
                if (newChild != child.Key || mergeNavigationProperties)
                {
                    if (child.Value.Multiplicity == NavigationPropertyMultiplicity.Many)
                    {
                        bool noctState = false, ooctState = false;
                        var noct = newChild as IObjectWithChangeTracker;
                        var ooct = child.Key as IObjectWithChangeTracker;
                        if (noct != null && ooct != null) {
                            noctState = noct.ChangeTracker.ChangeTrackingEnabled;
                            noct.ChangeTracker.ChangeTrackingEnabled = false;

                            ooctState = ooct.ChangeTracker.ChangeTrackingEnabled;
                            ooct.ChangeTracker.ChangeTrackingEnabled = false;
                        }

                        child.Value.AddItem(result, newChild);
                        if (newChild != child.Key)
                        {
                            child.Value.RemoveItem(result, child.Key);
                        }

                        if (noct != null && ooct != null)
                        {
                            noct.ChangeTracker.ChangeTrackingEnabled = noctState;

                            ooct.ChangeTracker.ChangeTrackingEnabled = ooctState;
                        }
                    }
                    else
                    {
                        child.Value.SetValue(result, newChild);
                    }
                }
            }

            if (newItem)
            {
                FixupProperties(entity, result);
                FixupReverseProperties(result, entity);
            }

            if (changeTrackerItem != null)
            {
                changeTrackerItem.StartTracking();
            }

            return result;
        }

        private void FixupProperties(EntityMetadata entity, object result)
        {
            var properties = entity.Properties.OfType<NavigationPropertyMetadata>()
                .Where(p => p.Multiplicity != NavigationPropertyMultiplicity.Many)
                .Where(p => p.GetValue(result) == null)
                .ToArray();

            foreach (var prop in properties)
            {
                var fixup = this.TrackedObjects.FirstOrDefault(p => prop.TargetEntity.IsOfType(p) && prop.MatchForeignKeys(result, p));

                if (fixup != null)
                {
                    var changTrackingFixup = fixup as IObjectWithChangeTracker;
                    if (changTrackingFixup != null)
                    {
                        changTrackingFixup.StopTracking();
                    }

                    prop.SetValue(result, fixup);
                    FixupPrincipal(prop, fixup, result);

                    if (changTrackingFixup != null)
                    {
                        changTrackingFixup.StartTracking();
                    }
                }
            }
        }

        private void FixupReverseProperties(object result, EntityMetadata entity)
        {
            var reverseProperties = this.Model.Entities
                .SelectMany(p => p.Properties.OfType<NavigationPropertyMetadata>())
                .Where(p => p.TargetEntity == entity && p.Multiplicity != NavigationPropertyMultiplicity.Many)
                .ToArray();

            foreach (var prop in reverseProperties)
            {
                var fixups =
                    this.TrackedObjects.Where(
                        p => prop.Entity.IsOfType(p) && prop.GetValue(p) == null && prop.MatchForeignKeys(p, result));

                foreach (var fixup in fixups)
                {
                    var changTrackingFixup = fixup as IObjectWithChangeTracker;
                    if (changTrackingFixup != null)
                    {
                        changTrackingFixup.StopTracking();
                    }

                    prop.SetValue(fixup, result);
                    FixupPrincipal(prop, result, fixup);

                    if (changTrackingFixup != null)
                    {
                        changTrackingFixup.StartTracking();
                    }
                }
            }
        }

        private static void FixupPrincipal(NavigationPropertyMetadata prop, object fixup, object source)
        {
            if (prop.TargetNavigationProperty != null && prop.TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.Many)
            {
                prop.TargetNavigationProperty.AddItem(fixup, source);
            }
        }

        private void UpdateScalarProperties(object source, object destination)
        {
            var scalarProperties = this.Model.GetEntityMetadata(source).Properties.OfType<ScalarProperty>();

            foreach (var property in scalarProperties)
            {
                if (!Object.Equals(property.GetValue(source), property.GetValue(destination)))
                {
                    property.SetValue(destination, property.GetValue(source));
                }
            }
        }

        public IEnumerable<T> Cache<T>(IEnumerable<T> items)
        {
            if (!CheckAccess())
            {
                IEnumerable<T> newItems = null;
                this.SynchronizationContext.Send(new SendOrPostCallback(p => newItems = Cache(items)), null);
                return newItems;
            }

            Dictionary<object, object> tuples = new Dictionary<object, object>();
            List<object> cleanedItems = new List<object>(TrackedObjects);

            var result = items.Select(item => (T)CleanUp(item, cleanedItems, tuples)).ToList();

            MergeScalarProperties(tuples);

            //result.OfType<IObjectWithChangeTracker>().ToList().ForEach(p => p.AcceptChangesRecursive());
            return result;
        }

        private void MergeScalarProperties(IDictionary<object, object> tuples)
        {
            foreach (var item in tuples)
            {
                bool tracking = false;
                var ioct = item.Value as IObjectWithChangeTracker;
                if (ioct != null)
                {
                    tracking = ioct.ChangeTracker.ChangeTrackingEnabled;
                    ioct.StopTracking();
                }

                UpdateScalarProperties(item.Key, item.Value);
            }
        }

        public T CacheSingle<T>(T item)
        {
            if (!CheckAccess())
            {
                T newItem = default(T);
                this.SynchronizationContext.Send(new SendOrPostCallback(p => newItem = CacheSingle(item)), null);
                return newItem;
            }

            Dictionary<object, object> tuples = new Dictionary<object, object>();

            var result = (T)CleanUp(item, new List<object>(TrackedObjects), tuples);

            MergeScalarProperties(tuples);

            var ioct = result as IObjectWithChangeTracker;
            //if (ioct != null)
            //    ioct.AcceptChangesRecursive();

            return default(T);
        }
    }
}
