using System.ServiceModel;

namespace Lucile.ServiceModel.DependencyInjection.Behavior
{
    public class MessageScopeExtension : IExtension<InstanceContext>
    {
        public MessageScopeExtension(MessageScopeCollection collection)
        {
            Collection = collection;
        }

        public MessageScopeCollection Collection { get; }

        public void Attach(InstanceContext owner)
        {
        }

        public void Detach(InstanceContext owner)
        {
        }
    }
}