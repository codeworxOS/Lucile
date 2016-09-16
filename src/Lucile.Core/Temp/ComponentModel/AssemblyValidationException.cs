using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.ComponentModel
{
    public class AssemblyValidationException : Exception
    {
        public AssemblyValidationException()
            : base("Error validating the assembly") { }

        public AssemblyValidationException(string message)
            : base(message) { }

        public AssemblyValidationException(string message, Exception inner)
            : base(message, inner) { }

#if(!SILVERLIGHT)
        protected AssemblyValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif
    }
}
