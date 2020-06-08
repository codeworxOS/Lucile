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

                    if (assemblyName != null)
                    {
                        assemblyName.Version = null;
                        _clrTypeName = $"{_clrType.FullName}, {assemblyName.Name}";
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
                    if (value == null)
                    {
                        _clrType = null;
                        _clrTypeName = null;
                    }
                    else
                    {
                        var index = GetSeparatorIndex(value);

                        var typeNameToken = value.Substring(0, index);
                        var assemblyNameToken = value.Substring(index + 1);
                        var assemblyName = new AssemblyName(assemblyNameToken);
                        _clrTypeName = $"{typeNameToken}, {assemblyName.Name}";
                        _clrType = Type.GetType(_clrTypeName);
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as ClrTypeInfo;
            if (typedObj != null)
            {
                return typedObj.ClrType.Equals(_clrType);
            }

            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
            return _clrType?.GetHashCode() ?? 0;
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'
        }

        private int GetSeparatorIndex(string value)
        {
            int genericBracetCount = 0;
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '[':
                        genericBracetCount++;
                        break;
                    case ']':
                        genericBracetCount--;
                        break;
                    case ',':
                        if (genericBracetCount == 0)
                        {
                            return i;
                        }

                        break;
                }
            }

            return value.Length - 1;
        }
    }
}