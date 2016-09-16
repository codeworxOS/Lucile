using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata
{
    [DataContract]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public class MetadataModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MetadataModel()
        {
            Entities = new List<EntityMetadata>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entities"></param>
        public MetadataModel(IEnumerable<EntityMetadata> entities)
        {
            Entities = new List<EntityMetadata>(entities);
        }

        /// <summary>
        /// Liefert oder setzt die Entities
        /// </summary>
        [DataMember(Order = 1)]
        public List<EntityMetadata> Entities
        {
            get;
            set;
        }

        /// <summary>
        /// Liefert die Metadaten einer Entität
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadata(object parameter)
        {
            var types = this.Entities.Where(p => p.IsOfType(parameter)).ToArray();

            return GetMetadata(types);
        }

        /// <summary>
        /// Liefert die Metadaten einer Entität
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadata<T>()
        {
            return GetEntityMetadata(typeof(T));
        }

        /// <summary>
        /// Liefert die Metadaten einer Entität
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadata(Type entityType)
        {
            var types = this.Entities.Where(p => p.ClrType.IsAssignableFrom(entityType)).ToArray();

            return GetMetadata(types);
        }

        private static EntityMetadata GetMetadata(EntityMetadata[] types)
        {
            if (types.Count() > 1)
            {
                var baseEntity = types.Where(p => p.BaseEntity == null).FirstOrDefault();
                while (types.Any(p => p.BaseEntity == baseEntity))
                {
                    baseEntity = types.Where(p => p.BaseEntity == baseEntity).FirstOrDefault();
                }

                return baseEntity;
            }
            else
            {
                return types.FirstOrDefault();
            }
        }
    }
}