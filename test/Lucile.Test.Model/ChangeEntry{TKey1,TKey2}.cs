using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lucile.Test.Model
{
    public class ChangeEntry<TKey1, TKey2> : ChangeEntry<TKey1>
    {
        public TKey2 Key2 { get; set; }
    }
}