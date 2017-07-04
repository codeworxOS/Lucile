using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace Lucile.ServiceModel
{
    [DataContract]
    public class ExceptionFault
    {
#if NET45
        private static readonly Action<Exception> _captureDelegate = null;

        static ExceptionFault()
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(Exception));
            var call = System.Linq.Expressions.Expression.Call(param, typeof(Exception).GetMethod("InternalPreserveStackTrace", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            _captureDelegate = System.Linq.Expressions.Expression.Lambda<Action<Exception>>(call, param).Compile();
        }
#endif

        public ExceptionFault()
        {
        }

        public ExceptionFault(string tracingIdentifier, Exception exception, bool includeDetails)
        {
            TracingIdentifier = tracingIdentifier;

            var type = exception.GetType();

            var assemblyName = type.GetTypeInfo().Assembly.GetName();
            var typeName = type.FullName;
            assemblyName.Version = null;

            this.ExceptionType = $"{typeName}, {assemblyName}";

            if (includeDetails)
            {
                Message = exception.Message;
                ExceptionInfo = exception.ToString();
                ExceptionPayload = Serialize(exception);
            }
        }

        [DataMember(Order = 5)]
        public string ExceptionInfo { get; set; }

        [DataMember(Order = 1)]
        public byte[] ExceptionPayload { get; set; }

        [DataMember(Order = 2)]
        public string ExceptionType { get; set; }

        [DataMember(Order = 3)]
        public string Message { get; set; }

        [DataMember(Order = 4)]
        public string TracingIdentifier { get; set; }

        public void ReThrow()
        {
            Exception ex = null;
            try
            {
                if (ExceptionPayload != null)
                {
                    ex = Deserialize(ExceptionPayload);
                }
            }
            catch
            {
                // do nothing
            }

            if (ex != null)
            {
                throw ex;
            }

            throw new ServiceProcessingException(this.TracingIdentifier, this.ExceptionType, this.Message, this.ExceptionInfo);
        }

        private static Exception Deserialize(byte[] exceptionPayload)
        {
#if NET45
            try
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var ms = new System.IO.MemoryStream(exceptionPayload))
                {
                    var ex = formatter.Deserialize(ms) as Exception;
                    return ex;
                }
            }
            catch
            {
                // do nothing
            }
#endif
            return null;
        }

        private static byte[] Serialize(Exception exception)
        {
#if NET45
            _captureDelegate(exception);
            try
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var ms = new System.IO.MemoryStream())
                {
                    formatter.Serialize(ms, exception);
                    return ms.ToArray();
                }
            }
            catch
            {
                // do nothing
            }
#endif
            return null;
        }
    }
}