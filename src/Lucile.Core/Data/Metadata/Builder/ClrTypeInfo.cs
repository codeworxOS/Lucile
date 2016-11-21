using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class ClrTypeInfo
    {
        private Type _clrType;

        private string _clrTypeName;

        public ClrTypeInfo(Type type)
        {
            ClrType = type;
        }

        public ClrTypeInfo()
        {
        }

        [IgnoreDataMember]
        public Type ClrType
        {
            get
            {
                return _clrType;
            }

            set
            {
                if (value != _clrType)
                {
                    _clrType = value;
                    _clrTypeName = _clrType?.AssemblyQualifiedName;
                }
            }
        }

        [DataMember(Order = 1)]
        public string ClrTypeName
        {
            get
            {
                return _clrTypeName;
            }

            set
            {
                if (_clrTypeName != value)
                {
                    _clrTypeName = value;
                    if (_clrTypeName == null)
                    {
                        _clrType = null;
                    }
                    else
                    {
                        _clrType = Type.GetType(_clrTypeName);
                    }
                }
            }
        }
    }
}