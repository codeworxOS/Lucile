using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    public interface ISyncService
    {
        string SyncMethodWithResult(string param1, int param2);

        void SyncVoidMethod(string param1, int param2);
    }
}