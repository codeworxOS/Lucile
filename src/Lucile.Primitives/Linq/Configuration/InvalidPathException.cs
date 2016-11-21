using System;

namespace Lucile.Linq.Configuration
{
    public class InvalidPathException : Exception
    {
        public InvalidPathException()
        {
        }

        public InvalidPathException(string message)
            : base(message)
        {
        }

        public InvalidPathException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidPathException(Type type, string property)
            : base($"Property {property} not found on Type [{type}]")
        {
        }
    }
}