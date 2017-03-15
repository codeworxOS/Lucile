namespace Lucile.ServiceModel
{
    public class RemoteServiceOptionsBuilder
    {
        public RemoteServiceOptionsBuilder()
        {
        }

        public string BaseAddress { get; set; }

        public long MaxMessageSize { get; set; }

        public RemoteServiceOptionsBuilder Base(string address)
        {
            BaseAddress = address;
            return this;
        }

        public RemoteServiceOptions ToOptions()
        {
            return new RemoteServiceOptions(this.BaseAddress, this.MaxMessageSize);
        }
    }
}