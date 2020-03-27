using System;

namespace Lucile.AspNetCore.Demo
{
    public class TestSettings
    {
        public string StringProp { get; set; }

        public int NumberProp { get; set; }

        public DateTime DateTimeProp { get; set; }

        public TestSubSetting Level2 { get; set; }
    }
}
