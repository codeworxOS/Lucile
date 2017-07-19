using System;
using System.Reflection;
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
                    var assemblyName = _clrType?.GetTypeInfo()?.Assembly?.GetName();
                    assemblyName.Version = null;

                    if (assemblyName != null)
                    {
                        _clrTypeName = $"{_clrType?.FullName}, {assemblyName}";
                    }
                    else
                    {
                        _clrTypeName = null;
                    }
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