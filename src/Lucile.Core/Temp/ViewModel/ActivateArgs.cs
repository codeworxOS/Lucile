using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.ViewModel
{
    public class ActivateArgs
    {
        public ActivateArgs(object previousContent)
        {
            this.PreviousContent = previousContent;
        }

        public object PreviousContent { get; private set; }
    }
}
