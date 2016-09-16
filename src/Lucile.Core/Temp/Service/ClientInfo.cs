using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.Service
{
    [DataContract(IsReference = true)]
    public class ClientInfo : IEquatable<ClientInfo>
    {
        [DataMember]
        public string DisplayName { get; set; }

        [IgnoreDataMember]
        public IPAddress Address
        {
            get
            {
                if (AddressData != null)
                    return new IPAddress(AddressData);

                return null;
            }
            set
            {
                if (value != null) {
                    this.AddressData = value.GetAddressBytes();
                } else {
                    this.AddressData = null;
                }
            }
        }

        [DataMember]
        public byte[] AddressData { get; set; }

        [DataMember]
        public DateTime Connected { get; set; }

        public override int GetHashCode()
        {
            return this.DisplayName != null ? this.DisplayName.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClientInfo;
            if (other != null)
                return this.Equals(other);

            return base.Equals(obj);
        }

        #region IEquatable<ClientInfo> Members

        public bool Equals(ClientInfo other)
        {
            return object.Equals(this.DisplayName, other.DisplayName);
        }

        #endregion
    }
}
