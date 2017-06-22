using System;

namespace Lucile
{
    public class ServiceProcessingException : Exception
    {
        public ServiceProcessingException()
            : this("Error processing service request.")
        {
        }

        public ServiceProcessingException(string exceptionType, string message, string info)
            : this($"Service request threw exception {exceptionType} ({message}).")
        {
            RemoteExceptionInfo = info;
        }

        public ServiceProcessingException(string message)
            : base(message)
        {
        }

        public ServiceProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string RemoteExceptionInfo { get; private set; }
    }
}