using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.ViewModel
{
    public class DeactivateArgs
    {
        public DeactivateArgs(object newContent)
        {
            NewContent = newContent;
        }

        public object NewContent { get; private set; }

        public bool Cancel { get; set; }
    }
}
