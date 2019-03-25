using System;

namespace Lucile.Service
{
    public class MissingDefaultConnectedException : Exception
    {
        public MissingDefaultConnectedException()
            : this("There is no IDefaultConneted registered in your DI container.")
        {
        }

        public MissingDefaultConnectedException(string message)
            : base(message)
        {
        }

        public MissingDefaultConnectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}