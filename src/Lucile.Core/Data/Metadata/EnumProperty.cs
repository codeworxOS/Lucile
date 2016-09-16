using System;
using System.Runtime.Serialization;
using Lucile.Reflection;

namespace Lucile.Data.Metadata
{
    [DataContract(IsReference = true)]
    public class EnumProperty : ScalarProperty
    {
        private Type _enumType;

        private string _enumTypeName;

        private Type _underlyingPrimitiveType;

        private string _underlyingPrimitiveTypeName;

        public EnumProperty(EntityMetadata entity)
            : base(entity)
        {
        }

        internal EnumProperty()
        {
        }

        [IgnoreDataMember]
        public Type EnumType
        {
            get
            {
                if (_enumType == null)
                {
                    _enumType = TypeResolver.GetType(this.EnumTypeName);
                }

                return _enumType;
            }

            set
            {
                this.EnumTypeName = value != null ? value.AssemblyQualifiedName : null;
                _enumType = value;
            }
        }

        [DataMember(Order = 1)]
        public string EnumTypeName
        {
            get
            {
                return _enumTypeName;
            }

            set
            {
                this._enumTypeName = value;
                this._enumType = null;
            }
        }

        [DataMember(Order = 3)]
        public bool IsFlag { get; set; }

        [IgnoreDataMember]
        public Type UnderlyingPrimitiveType
        {
            get
            {
                if (_underlyingPrimitiveType == null)
                {
                    _underlyingPrimitiveType = TypeResolver.GetType(this.UnderlyingPrimitiveTypeName);
                }

                return _underlyingPrimitiveType;
            }

            set
            {
                this.UnderlyingPrimitiveTypeName = value != null ? value.AssemblyQualifiedName : null;
                _underlyingPrimitiveType = value;
            }
        }

        [DataMember(Order = 2)]
        public string UnderlyingPrimitiveTypeName
        {
            get
            {
                return _underlyingPrimitiveTypeName;
            }

            set
            {
                this._underlyingPrimitiveTypeName = value;
                this._underlyingPrimitiveType = null;
            }
        }
    }
}