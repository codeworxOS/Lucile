using System;

namespace Lucile.Json
{
    public class JsonInheritanceConverter
    {
        internal static readonly string DefaultDiscriminatorName = "discriminator";

        private readonly Type _baseType;
        private readonly string _discriminator;
        private readonly bool _readTypeProperty;

        public JsonInheritanceConverter()

            : this(DefaultDiscriminatorName, false)
        {
        }

        public JsonInheritanceConverter(string discriminator)

            : this(discriminator, false)
        {
        }

        public JsonInheritanceConverter(string discriminator, bool readTypeProperty)
        {
            _discriminator = discriminator;
            _readTypeProperty = readTypeProperty;
        }

        public JsonInheritanceConverter(Type baseType)

            : this(baseType, DefaultDiscriminatorName)
        {
        }

        public JsonInheritanceConverter(Type baseType, string discriminator)
            : this(discriminator, false)
        {
            _baseType = baseType;
        }

        public virtual string DiscriminatorName => _discriminator;

        public virtual string GetDiscriminatorValue(Type type)
        {
            return type.Name;
        }
    }
}