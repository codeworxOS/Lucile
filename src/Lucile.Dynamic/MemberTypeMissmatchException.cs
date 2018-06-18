using System;

namespace Lucile.Dynamic
{
    public class MemberTypeMissmatchException : Exception
    {
        public MemberTypeMissmatchException()
            : base("The types of the base member does not match the type of the inharited member.")
        {
        }

        public MemberTypeMissmatchException(string memberName, string message = null)
            : base(string.Format("The types of the base member [{0}] does not match the type of the inharited member.\r\n{1}", memberName, message))
        {
            this.MemberName = memberName;
        }

        public string MemberName { get; private set; }
    }
}