using System;

namespace Lucile.Data.Metadata.Builder
{
    public class ModelBuilderValidationExcpetion : Exception
    {
        public ModelBuilderValidationExcpetion(string message)
            : base(message)
        {
        }

        public ModelBuilderValidationExcpetion(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}