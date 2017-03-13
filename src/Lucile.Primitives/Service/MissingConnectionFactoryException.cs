using System;

namespace Lucile.Service
{
    public class MissingConnectionFactoryException : Exception
    {
        public MissingConnectionFactoryException()
            : this("There is no IConnectionFacotry registered in your DI container.")
        {
        }

        public MissingConnectionFactoryException(string message)
            : base(message)
        {
        }

        public MissingConnectionFactoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}