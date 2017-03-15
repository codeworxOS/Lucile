namespace Lucile.ServiceModel
{
    public class RemoteServiceOptionsBuilder
    {
        public RemoteServiceOptionsBuilder()
        {
            this.MaxMessageSize = 1024 * 64;
        }

        public string BaseAddress { get; set; }

        public long MaxMessageSize { get; set; }

        public RemoteServiceOptionsBuilder Base(string address)
        {
            BaseAddress = address;
            return this;
        }

        public RemoteServiceOptionsBuilder Size(long maxMessageSize)
        {
            MaxMessageSize = maxMessageSize;
            return this;
        }

        public RemoteServiceOptions ToOptions()
        {
            return new RemoteServiceOptions(this.BaseAddress, this.MaxMessageSize);
        }
    }
}