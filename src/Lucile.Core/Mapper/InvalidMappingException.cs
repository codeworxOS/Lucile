using System;

namespace Lucile.Mapper
{
    ////[Serializable]
    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(Type sourceType, Type targetType)
            : base(string.Format("Multiple or no mappingConfiguration found for source type [{0}] and target type [{1}].", sourceType, targetType))
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
        }

        public Type SourceType { get; private set; }

        public Type TargetType { get; private set; }
    }
}