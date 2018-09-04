using System.Windows.Data;
using System.Windows.Threading;
using Lucile.Windows.Converter;

namespace Lucile.Windows.Markup
{
    public class CommandProxyBinding : Binding
    {
        public CommandProxyBinding()
            : base()
        {
            this.Converter = new CommandConverter(Dispatcher.CurrentDispatcher);
        }

        public CommandProxyBinding(string path)
            : base(path)
        {
            this.Converter = new CommandConverter(Dispatcher.CurrentDispatcher);
        }
    }
}