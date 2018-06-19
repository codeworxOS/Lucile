using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucile.Dynamic.Test.Proxy
{
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string memberName)
        {
            this.MemberName = memberName;
        }

        public string MemberName { get; private set; }
    }
}