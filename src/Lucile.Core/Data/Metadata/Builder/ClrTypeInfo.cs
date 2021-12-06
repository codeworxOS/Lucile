using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class ClrTypeInfo
    {
        private static readonly object _systemAssembly;
        private Type _clrType;
        private string _clrTypeName;

        static ClrTypeInfo()
        {
            _systemAssembly = typeof(object).GetTypeInfo().Assembly;
        }

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
                    if (_clrType != null)
                    {
                        _clrTypeName = GetTypeName(_clrType);
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

                        if (index > 0)
                        {
                            var typeNameToken = value.Substring(0, index);
                            var assemblyNameToken = value.Substring(index + 1);
                            var assemblyName = new AssemblyName(assemblyNameToken);
                            _clrTypeName = $"{typeNameToken}, {assemblyName.Name}";
                        }
                        else
                        {
                            _clrTypeName = value;
                        }

                        _clrType = Type.GetType(_clrTypeName);
                    }
                }
            }
        }

        public ClrTypeInfo Clone()
        {
            return new ClrTypeInfo
            {
                _clrType = _clrType,
                _clrTypeName = _clrTypeName
            };
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as ClrTypeInfo;
            if (typedObj != null)
            {
                return typedObj.ClrType?.Equals(_clrType) ?? typedObj.ClrTypeName.Equals(_clrTypeName);
            }

            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
            return _clrType?.GetHashCode() ?? _clrTypeName?.GetHashCode() ?? 0;
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'
        }

        private static int GetSeparatorIndex(string value)
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

            return -1;
        }

        private static string GetTypeName(Type clrType)
        {
            var result = $"{clrType.Namespace}.{clrType.Name}";

            if (clrType.GetTypeInfo().IsGenericType)
            {
                result += $"[{string.Join(", ", clrType.GetGenericArguments().Select(p => $"[{GetTypeName(p)}]"))}]";
            }

#pragma warning disable CS0253 // totally intentional
            if (clrType.GetTypeInfo().Assembly == _systemAssembly)
#pragma warning restore CS0253 // totally intentional
            {
                return result;
            }

            var assemblyName = clrType.GetTypeInfo().Assembly.GetName();
            result += $", {assemblyName.Name}";

            return result;
        }
    }
}