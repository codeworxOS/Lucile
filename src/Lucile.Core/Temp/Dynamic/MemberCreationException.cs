using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public class MemberCreationException : Exception
    {
        public string MemberName { get; private set; }

        public MemberCreationException()
            : base("Member could not be created.")
        {

        }

        public MemberCreationException(string memberName, string message = null)
            : base(string.Format("Member {0} could not be created.\r\n{1}", memberName, message))
        {
            this.MemberName = memberName;
        }
    }
}
