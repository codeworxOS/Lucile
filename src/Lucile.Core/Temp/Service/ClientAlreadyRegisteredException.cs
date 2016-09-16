using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public class ClientAlreadyRegisteredException : Exception
    {
        public string ClientIdentity { get; set; }

        public ClientAlreadyRegisteredException()
            : base("A client with the same identity is already registered")
        {

        }

        public ClientAlreadyRegisteredException(string clientIdentity)
            : this(clientIdentity, string.Format("A client with the identity [{0}] is already registerd.",clientIdentity))
        { }

        public ClientAlreadyRegisteredException(string clientIdentity, string message)
            : base(message)
        {
            this.ClientIdentity = clientIdentity;
        }

        public ClientAlreadyRegisteredException(string clientIdentity, string message, Exception inner)
            : base(message, inner)
        {
            this.ClientIdentity = clientIdentity;
        }
    }
}
