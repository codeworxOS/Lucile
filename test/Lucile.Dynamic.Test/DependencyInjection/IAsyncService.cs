using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    public interface IAsyncService
    {
        Task<string> AsyncMethodWithResult(string param1, int param2);

        Task AsyncVoidMethod(string param1, int param2);
    }
}