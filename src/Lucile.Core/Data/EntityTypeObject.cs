using System;
using System.Runtime.Serialization;
using Lucile.Reflection;

namespace Lucile.Data
{
    [DataContract(IsReference = true)]
    public class EntityTypeObject
    {
        private Type _entityType = null;
        private string _entityTypeName = null;

        [DataMember(Order = 1)]
        public string EntityTypeName
        {
            get
            {
                return _entityTypeName;
            }

            set
            {
                this._entityTypeName = value;
                this._entityType = null;
            }
        }

        [IgnoreDataMember]
        public Type EntityType
        {
            get
            {
                if (_entityType == null)
                {
                    _entityType = TypeResolver.GetType(this.EntityTypeName);
                }

                return _entityType;
            }

            set
            {
                this.EntityTypeName = value != null ? value.AssemblyQualifiedName : null;
                _entityType = value;
            }
        }
    }
}