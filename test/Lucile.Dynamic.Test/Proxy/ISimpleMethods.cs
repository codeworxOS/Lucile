using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Proxy
{
    public interface ISimpleMethods
    {
        TResult GenericText<TResult>(TResult test, string bla);

        int Integer();

        string String();

        string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13);

        string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8);

        string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9);

        string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10);

        string StringWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11);

        string StringWithParameters(string param1, int param2, TestClass param3);

        TestClass TestClass();

        TestClass TestClassWithParameters(string param1, int param2, TestClass param3);

        void Void();

        void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8);

        void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9);

        void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10);

        void VoidWithManyParameters(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11);

        void VoidWithParameters(string param1, int param2, TestClass param3);
    }

    public interface ISimpleMethodsAsync
    {
        Task<TResult> GenericText<TResult>(TResult test, string bla);

        Task<int> Integer();

        Task<string> String();

        Task<string> StringWithManyParametersAsync(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13);

        Task<string> StringWithParameters(string param1, int param2, TestClass param3);

        Task<TestClass> TestClass();

        Task<TestClass> TestClassWithParameters(string param1, int param2, TestClass param3);

        Task Void();

        Task VoidWithParameters(string param1, int param2, TestClass param3);
    }
}