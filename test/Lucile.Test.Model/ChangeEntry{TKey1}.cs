using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lucile.Test.Model
{
    public class ChangeEntry<TKey1> : ChangeEntry
    {
        public TKey1 Key1 { get; set; }
    }
}