using System.ServiceModel.Channels;

namespace Lucile.ServiceModel.DependencyInjection
{
    public interface IMessageScopeInitializer
    {
        IMessageScope Initialize(ref Message message);
    }
}