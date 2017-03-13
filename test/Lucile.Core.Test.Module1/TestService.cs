using System;
using System.Threading.Tasks;

namespace Lucile.Core.Test.Module1
{
    public class TestService : ITestService
    {
        public Task<TestData> GetTestDataAsync(int id)
        {
            return Task.FromResult(new TestData { Id = id, Name = "Test", Date = DateTime.Today });
        }
    }
}