using System;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.Test.Proxy;
using Xunit;

namespace Lucile.Dynamic.Text
{
    public class DynamicProxyTest
    {
        [Fact]
        public void DynamicProxyTestSimpleMethods()
        {
            var simpleMethods = new SimpleMethods();

            string lastCalled = string.Empty;

            simpleMethods.GotCalled += (s, e) => lastCalled = e.MemberName;

            var dtb = new DynamicTypeBuilder();
            dtb.AddConvention(new ProxyConvention<ISimpleMethods>());
            var inst = Activator.CreateInstance(dtb.GeneratedType) as ISimpleMethods;

            Assert.NotNull(inst);
            Assert.True(inst is IDynamicProxy);

            ((IDynamicProxy)inst).SetProxyTarget<ISimpleMethods>(simpleMethods);

            inst.Void();
            Assert.Equal("Void", lastCalled);

            inst.VoidWithParameters("Void", 0, new TestClass());
            Assert.Equal("VoidWithParameters", lastCalled);

            inst.String();
            Assert.Equal("String", lastCalled);

            inst.StringWithParameters("StringWithParameters", 0, new TestClass());
            Assert.Equal("StringWithParameters", lastCalled);

            inst.TestClass();
            Assert.Equal("TestClass", lastCalled);

            inst.TestClassWithParameters("TestClassWithParameters", 0, new TestClass());
            Assert.Equal("TestClassWithParameters", lastCalled);
        }
    }
}