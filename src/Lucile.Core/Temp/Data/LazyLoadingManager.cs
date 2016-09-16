using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codeworx.Core.Service;
using Codeworx.Data.Metadata;

namespace Codeworx.Data
{
    public class LazyLoadingManager<TService> where TService : class, ILazyLoadingService
    {
        public MetadataModel Model { get; private set; }

        public LazyLoadingManager(MetadataModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Model = model;
        }

        public Task LoadProperties<TEntity>(TEntity entity, IncludePaths paths, CancellationToken token = default(CancellationToken))
        {
            return LoadProperties<TEntity>(new[] { entity }, paths, token);
        }

        public async Task LoadProperties<TEntity>(IEnumerable<TEntity> entities, IncludePaths paths, CancellationToken token = default(CancellationToken))
        {
            var meta = this.Model.GetEntityMetadata<TEntity>();

            if (meta == null) {
                throw new ArgumentOutOfRangeException("TEntity", "The entity type is not part of the given MetadataModel.");
            }

            var keys = entities.Select(p => meta.GetEntityKey(p)).Distinct().ToList();

            if (token.IsCancellationRequested)
                return;

            var service = await ServiceContext.Current.GetServiceAsync<TService>(token);

            if (token.IsCancellationRequested)
                return;

            var response = await service.GetPropertyValuesAsync(keys, paths);

            if (token.IsCancellationRequested)
                return;

            foreach (var item in response) {
                var entity = this.Model.GetEntityMetadata(item.EntityType);

                var prop = entity.Properties.OfType<NavigationPropertyMetadata>().SingleOrDefault(p => p.Name == item.PropertyName);
                if (prop == null) {
                    throw new InvalidOperationException(string.Format("The NavigationProperty {0} could not be found on type {1}", item.PropertyName, item.EntityType));
                }

                if (prop.Multiplicity == NavigationPropertyMultiplicity.Many && prop.TargetNavigationProperty.Multiplicity != NavigationPropertyMultiplicity.Many) {
                    foreach (var nav in item.Values.Select(p => p.Value)) {
                        foreach (var e in entities) {
                            if (prop.TargetNavigationProperty.MatchForeignKeys(nav, e)) {
                                //TODO handle Null Value
                                prop.AddItem(e, nav);
                            }
                        }
                    }
                } else if (prop.Multiplicity == NavigationPropertyMultiplicity.Many && prop.TargetNavigationProperty.Multiplicity == NavigationPropertyMultiplicity.Many) {
                    //TODO implement    
                } else {
                    foreach (var nav in item.Values.Select(p => p.Value)) {
                        foreach (var e in entities) {
                            if (prop.MatchForeignKeys(e, nav)) {
                                prop.SetValue(e, nav);
                            }
                        }
                    }
                }
            }
        }
    }
}
