using System;

namespace Lucile
{
    public class ServiceProcessingException : Exception
    {
        public ServiceProcessingException(string tracingIdentifier)
            : this(tracingIdentifier, "Error processing service request.")
        {
        }

        public ServiceProcessingException(string tracingIdentifier, string exceptionType, string message, string info)
            : this(tracingIdentifier, $"Service request threw exception {exceptionType} ({message}).")
        {
            RemoteExceptionInfo = info;
        }

        public ServiceProcessingException(string tracingIdentifier, string message)
            : base(message)
        {
            this.TracingIdentifier = tracingIdentifier;
        }

        public ServiceProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string RemoteExceptionInfo { get; private set; }

        public string TracingIdentifier { get; private set; }
    }
}