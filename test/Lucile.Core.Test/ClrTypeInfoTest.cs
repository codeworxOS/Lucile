using Lucile.Data.Metadata.Builder;
using Lucile.Signed.Test.Model;
using Lucile.Test.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Lucile.Core.Test
{
    public partial class ClrTypeInfoTest
    {
        [Fact]
        void DeserializeMissmatchingVersionNumber()
        {
            var info = new ClrTypeInfo();
            info.ClrTypeName = "Lucile.Signed.Test.Model.ClrTypeSample, Lucile.Signed.Test.Model, Version=9.9.9.9, Culture=neutral, PublicKeyToken=d9998064f7e191e3";


            Assert.Equal(typeof(ClrTypeSample), info.ClrType);
        }

        [Fact]
        void DeserializeMissmatchingPublicKeyToken()
        {
            var info = new ClrTypeInfo();
            info.ClrTypeName = "Lucile.Signed.Test.Model.ClrTypeSample, Lucile.Signed.Test.Model, PublicKeyToken=b77a5c561934e089";


            Assert.Equal(typeof(ClrTypeSample), info.ClrType);
        }

        [Fact]
        void SerializeName()
        {
            var info = new ClrTypeInfo(typeof(ClrTypeSample));
            
            Assert.Equal("Lucile.Signed.Test.Model.ClrTypeSample, Lucile.Signed.Test.Model", info.ClrTypeName);
        }
    }
}
