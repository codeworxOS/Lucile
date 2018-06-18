using System;

namespace Lucile.Dynamic
{
    public class MemberCreationException : Exception
    {
        public MemberCreationException()
            : base("Member could not be created.")
        {
        }

        public MemberCreationException(string memberName, string message = null)
            : base(string.Format("Member {0} could not be created.\r\n{1}", memberName, message))
        {
            this.MemberName = memberName;
        }

        public string MemberName { get; private set; }
    }
}