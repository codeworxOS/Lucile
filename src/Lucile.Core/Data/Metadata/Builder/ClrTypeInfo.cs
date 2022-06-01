using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class ClrTypeInfo : IEquatable<ClrTypeInfo>
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

        // this is second one '!='
        public static bool operator !=(ClrTypeInfo obj1, ClrTypeInfo obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==(ClrTypeInfo obj1, ClrTypeInfo obj2)
        {
            if (object.ReferenceEquals(obj1, null) && object.ReferenceEquals(obj2, null))
            {
                return true;
            }
            else if (object.ReferenceEquals(obj1, null) || object.ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
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
                return Equals(typedObj);
            }

            return object.ReferenceEquals(this, obj);
        }

        public bool Equals(ClrTypeInfo other)
        {
            return other._clrType?.Equals(_clrType) ?? other._clrTypeName.Equals(_clrTypeName);
        }

        public string GetFriendlyName()
        {
            if (_clrType != null)
            {
                return _clrType.GetFriendlyName();
            }

            return ParseTypeName(_clrTypeName);
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

        private string ParseTypeName(string clrTypeName)
        {
            var index = GetSeparatorIndex(clrTypeName);
            var type = clrTypeName;

            if (index > 0)
            {
                type = clrTypeName.Substring(0, index);
            }

            var genericIndex = type.IndexOf("[");

            if (genericIndex >= 0)
            {
                var name = type.Substring(0, type.IndexOf('`'));
                name = name.Split('.').Last();

                List<string> generics = new List<string>();
                string currentGeneric = null;
                int bracetCount = 0;
                for (int i = genericIndex + 1; i < type.Length; i++)
                {
                    if (bracetCount > 0)
                    {
                        if (type[i] == '[')
                        {
                            bracetCount++;
                        }
                        else if (type[i] == ']')
                        {
                            bracetCount--;
                        }

                        if (bracetCount > 0)
                        {
                            currentGeneric += type[i];
                        }
                        else
                        {
                            generics.Add(ParseTypeName(currentGeneric));
                        }
                    }
                    else if (type[i] == '[')
                    {
                        bracetCount++;
                        currentGeneric = string.Empty;
                    }
                }

                return $"{name}<{string.Join(",", generics)}>";
            }

            return type.Split('.').Last();
        }
    }
}