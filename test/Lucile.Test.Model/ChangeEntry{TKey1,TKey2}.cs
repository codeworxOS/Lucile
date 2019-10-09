using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lucile.Test.Model
{
    public class ChangeEntry<TKey1, TKey2> : ChangeEntry
    {
        public TKey2 Key2 { get; set; }

        public TKey1 Key1 { get; set; }
    }
}