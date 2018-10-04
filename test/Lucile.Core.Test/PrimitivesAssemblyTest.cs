using System;
using System.Collections.Generic;
using System.Text;
using Lucile.Data;
using Xunit;

namespace Tests
{
    public class PrimitivesAssemblyTest
    {
        [Fact]
        public void LoadPrimitivesAssembly()
        {
            var test = typeof(DynamicTuple).Assembly;
            Assert.NotEmpty(test.GetTypes());
        }
    }
}