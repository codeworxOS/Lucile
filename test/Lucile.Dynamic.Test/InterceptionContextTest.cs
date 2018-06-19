using System;
using System.Threading.Tasks;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.Interceptor;
using Lucile.Dynamic.Test.Dynamic;
using Lucile.Dynamic.Test.Proxy;
using Xunit;

namespace Lucile.Dynamic.Test
{
    public class InterceptionContextTest
    {
        [Fact]
        public async Task InterceptionContextAsyncProxyInterceptionTest()
        {
            var instance = new SimpleMethodsAsync();
            bool beforeStringCalled = false, afterStringCalled = false, insteadOfStringCalled = false;

            object[] arguments = null;

            var dtb = new DynamicTypeBuilder<DynamicAsyncProxyBase>();
            dtb.AddConvention(new ProxyConvention<ISimpleMethodsAsync>());

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as ISimpleMethodsAsync;

            ((IDynamicProxy)proxy).SetProxyTarget<ISimpleMethodsAsync>(instance);

            ((DynamicAsyncProxyBase)proxy).InterceptorCalled += (m, t) =>
            {
                if (m == InterceptionMode.BeforeBody && t.MemberName == "StringWithParameters")
                {
                    beforeStringCalled = true;
                    arguments = t.Arguments;
                }
                if (m == InterceptionMode.AfterBody && t.MemberName == "StringWithParameters")
                    afterStringCalled = true;
                if (m == InterceptionMode.InsteadOfBody && t.MemberName == "StringWithParameters")
                    insteadOfStringCalled = true;
            };

            var result = await proxy.StringWithParameters("TestString", 1, null);
            await proxy.Void();
            await proxy.VoidWithParameters("test", 123, null);
            await proxy.Integer();
            await proxy.String();
            await proxy.TestClass();
            await proxy.TestClassWithParameters("test", 2, null);

            var generic = await proxy.GenericText<int>(123, "test");
            var generic2 = await proxy.GenericText<string>("Testchen", "test");

            Assert.Equal(generic, 123);

            Assert.Equal("Testchen", generic2);

            Assert.True(beforeStringCalled);
            Assert.True(afterStringCalled);
            Assert.True(insteadOfStringCalled);

            Assert.Equal(3, arguments.Length);
            Assert.Equal("TestString", arguments[0]);
            Assert.Equal("TestString", result);
        }

        [Fact]
        public async Task InterceptionContextAsyncRegisterInsteadOfInterceptorsTest()
        {
            bool before1 = false, exception = false, after1 = false;

            var context = new AsyncInterceptionContext(this, "TestMethod", (Func<string, Task<string>>)TestMethodAsync, "TestValue");

            context.RegisterInterceptor(new DelegateAsyncMethodInterceptor(async c => before1 = true, InterceptionMode.BeforeBody));
            context.RegisterInterceptor(new DelegateAsyncMethodInterceptor(async c => c.SetResult("Result"), InterceptionMode.InsteadOfBody));
            try
            {
                context.RegisterInterceptor(new DelegateAsyncMethodInterceptor(async c => c.SetResult("whatever"), InterceptionMode.InsteadOfBody));
            }
            catch (ArgumentException ex)
            {
                exception = ex.ParamName == "interceptor";
            }
            context.RegisterInterceptor(new DelegateAsyncMethodInterceptor(async c => after1 = true, InterceptionMode.AfterBody));

            var result = await context.ExecuteAsync<string>();

            Assert.True(before1);
            Assert.True(exception);
            Assert.Equal("Result", result);
            Assert.True(after1);
        }

        [Fact]
        public async Task InterCeptionContextInsteadOfWithValueTypes()
        {
            var context = new AsyncInterceptionContext(null, "test", new Func<Task<bool>>(async () => false));

            context.RegisterInterceptor(new DelegateAsyncMethodInterceptor(async c => c.SetResult(Activator.CreateInstance(c.ReturnType.GetGenericArguments()[0])), InterceptionMode.InsteadOfBody));

            var result = await context.ExecuteAsync<bool>();

            Assert.False(result);
        }

        [Fact]
        public void InterceptionContextProxyInterceptionTest()
        {
            var instance = new SimpleMethods();
            bool beforeStringCalled = false, afterStringCalled = false, insteadOfStringCalled = false;

            object[] arguments = null;

            var dtb = new DynamicTypeBuilder<DynamicProxyBase>();
            dtb.AddConvention(new ProxyConvention<ISimpleMethods>());

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as ISimpleMethods;

            //var dtb2 = new DynamicTypeBuilder<DynamicProxyBase>();
            //dtb2.AddMember(new DynamicProperty("StringProperty", typeof(string)));
            //var proxy2 = Activator.CreateInstance(dtb2.GeneratedType) as ISimpleMethods;

            ((IDynamicProxy)proxy).SetProxyTarget<ISimpleMethods>(instance);

            ((DynamicProxyBase)proxy).InterceptorCalled += (m, t) =>
            {
                if (m == InterceptionMode.BeforeBody && t.MemberName == "StringWithParameters")
                {
                    beforeStringCalled = true;
                    arguments = t.Arguments;
                }
                if (m == InterceptionMode.AfterBody && t.MemberName == "StringWithParameters")
                    afterStringCalled = true;
                if (m == InterceptionMode.InsteadOfBody && t.MemberName == "StringWithParameters")
                    insteadOfStringCalled = true;
            };

            var result = proxy.StringWithParameters("TestString", 1, null);
            proxy.Void();
            proxy.VoidWithParameters("test", 123, null);
            var intTest = proxy.Integer();
            proxy.String();
            proxy.TestClass();
            proxy.TestClassWithParameters("test", 2, null);
            var generic = proxy.GenericText<int>(123, "test");
            var generic2 = proxy.GenericText<string>("testchen", "test");

            Assert.Equal(123, generic);
            Assert.Equal("testchen", generic2);

            Assert.True(beforeStringCalled);
            Assert.True(afterStringCalled);
            Assert.True(insteadOfStringCalled);

            Assert.Equal(3, arguments.Length);
            Assert.Equal("TestString", arguments[0]);
            Assert.Equal("TestString", result);
        }

        [Fact]
        public void InterceptionContextRegisterBeforeAndAfterInterceptorsTest()
        {
            bool before1 = false, before2 = false, after1 = false;

            var context = new InterceptionContext(this, "TestMethod", (Func<string, string>)TestMethod, "TestValue");

            context.RegisterInterceptor(new DelegateMethodInterceptor(c => before1 = true, InterceptionMode.BeforeBody));
            context.RegisterInterceptor(new DelegateMethodInterceptor(c => before2 = true, InterceptionMode.BeforeBody));
            context.RegisterInterceptor(new DelegateMethodInterceptor(c => after1 = true, InterceptionMode.AfterBody));

            context.Execute();

            Assert.True(before1);
            Assert.True(before2);
            Assert.Equal("TestValue", context.Result);
            Assert.True(after1);
        }

        [Fact]
        public void InterceptionContextRegisterInsteadOfInterceptorsTest()
        {
            bool before1 = false, exception = false, after1 = false;

            var context = new InterceptionContext(this, "TestMethod", (Func<string, string>)TestMethod, "TestValue");

            context.RegisterInterceptor(new DelegateMethodInterceptor(c => before1 = true, InterceptionMode.BeforeBody));
            context.RegisterInterceptor(new DelegateMethodInterceptor(c => c.SetResult("Result"), InterceptionMode.InsteadOfBody));
            try
            {
                context.RegisterInterceptor(new DelegateMethodInterceptor(c => c.SetResult("whatever"), InterceptionMode.InsteadOfBody));
            }
            catch (ArgumentException ex)
            {
                exception = ex.ParamName == "interceptor";
            }
            context.RegisterInterceptor(new DelegateMethodInterceptor(c => after1 = true, InterceptionMode.AfterBody));

            context.Execute();

            Assert.True(before1);
            Assert.True(exception);
            Assert.Equal("Result", context.Result);
            Assert.True(after1);
        }

#pragma warning disable 1998
#pragma warning restore 1998

        [Fact]
        public void InterceptionContextRegisterInsteadOfWithBaseCallInterceptorsTest()
        {
            var context = new InterceptionContext(this, "TestMethod", (Func<string, string>)TestMethod, "TestValue");

            context.RegisterInterceptor(new DelegateMethodInterceptor(InsteadOfBodyInterceptor, InterceptionMode.InsteadOfBody));

            context.Execute();

            Assert.Equal("TestValueResult", context.Result);
        }

        private void InsteadOfBodyInterceptor(InterceptionContext obj)
        {
            obj.SetResult((string)obj.ExecuteBody() + "Result");
        }

        private string TestMethod(string testParameter)
        {
            return testParameter;
        }

        private Task<string> TestMethodAsync(string testParameter)
        {
            return Task.Run(() => testParameter);
        }
    }
}