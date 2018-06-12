using System.Threading.Tasks;

namespace Tests
{
    public class MessageScopeService : IMessageScopeService
    {
        private readonly SampleInfo _info;

        public MessageScopeService(SampleInfo info)
        {
            this._info = info;
        }

        public async Task<string> TestAsync()
        {
            return _info.Info;
        }
    }
}