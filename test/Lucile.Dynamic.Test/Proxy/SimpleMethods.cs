using System;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Proxy
{
    public class SimpleMethods : ISimpleMethods
    {
        public event EventHandler<StringEventArgs> GotCalled;

        public string String()
        {
            OnGotCalled("String");
            return "String";
        }

        public string StringWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("StringWithParameters");
            return param1;
        }

        public TestClass TestClass()
        {
            OnGotCalled("TestClass");
            return new TestClass();
        }

        public TestClass TestClassWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("TestClassWithParameters");
            return param3;
        }

        public void Void()
        {
            OnGotCalled("Void");
        }

        public void VoidWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("VoidWithParameters");
        }

        protected virtual void OnGotCalled(string memberName)
        {
            if (this.GotCalled != null)
            {
                this.GotCalled(this, new StringEventArgs(memberName));
            }
        }

        #region ISimpleMethods Members

        public int Integer()
        {
            OnGotCalled("Integer");
            return 10;
        }

        #endregion ISimpleMethods Members

        #region ISimpleMethods Members

        public TResult GenericText<TResult>(TResult test, string bla)
        {
            return test;
        }

        #endregion ISimpleMethods Members

        #region ISimpleMethods Members

        public string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13)
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
                param1,
                param2,
                param3,
                param4,
                param5,
                param6,
                param7,
                param8,
                param9,
                param10,
                param11,
                param12,
                param13
                );
        }

        #endregion ISimpleMethods Members

        #region ISimpleMethods Members

        public string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8)
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
    param1,
    param2,
    param3,
    param4,
    param5,
    param6,
    param7,
    param8
    );
        }

        public string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9)
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
    param1,
    param2,
    param3,
    param4,
    param5,
    param6,
    param7,
    param8,
    param9
    );
        }

        public string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10)
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
    param1,
    param2,
    param3,
    param4,
    param5,
    param6,
    param7,
    param8,
    param9,
    param10
    );
        }

        public string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11)
        {
            return string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
    param1,
    param2,
    param3,
    param4,
    param5,
    param6,
    param7,
    param8,
    param9,
    param10,
    param11
    );
        }

        public void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8)
        {
        }

        public void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9)
        {
        }

        public void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10)
        {
        }

        public void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11)
        {
        }

        #endregion ISimpleMethods Members
    }

    public class SimpleMethodsAsync : ISimpleMethodsAsync
    {
        public event EventHandler<StringEventArgs> GotCalled;

        public async Task<string> String()
        {
            OnGotCalled("String");
            return "String";
        }

        public async Task<string> StringWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("StringWithParameters");
            return param1;
        }

        public async Task<TestClass> TestClass()
        {
            OnGotCalled("TestClass");
            return new TestClass();
        }

        public async Task<TestClass> TestClassWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("TestClassWithParameters");
            return param3;
        }

        public async Task Void()
        {
            OnGotCalled("Void");
        }

        public async Task VoidWithParameters(string param1, int param2, TestClass param3)
        {
            OnGotCalled("VoidWithParameters");
        }

        protected virtual void OnGotCalled(string memberName)
        {
            if (this.GotCalled != null)
            {
                this.GotCalled(this, new StringEventArgs(memberName));
            }
        }

#pragma warning disable 1998

        #region ISimpleMethods Members

        public async Task<int> Integer()
        {
            OnGotCalled("Integer");
            return 10;
        }

#pragma warning restore 1998

        #endregion ISimpleMethods Members

        #region ISimpleMethodsAsync Members

        public Task<TResult> GenericText<TResult>(TResult test, string bla)
        {
            return Task.Run(() => test);
        }

        #endregion ISimpleMethodsAsync Members

        #region ISimpleMethodsAsync Members

        public Task<string> StringWithManyParametersAsync(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13)
        {
            return Task.Run(() => string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}",
                param1,
                param2,
                param3,
                param4,
                param5,
                param6,
                param7,
                param8,
                param9,
                param10,
                param11,
                param12,
                param13
                ));
        }

        #endregion ISimpleMethodsAsync Members
    }
}