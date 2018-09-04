using System;
using System.Windows.Data;
using System.Windows.Threading;
using Lucile.Input;
using Lucile.Windows.Threading;
using Sade.Windows.Input;

namespace Lucile.Windows.Converter
{
    public class CommandConverter : IValueConverter
    {
        private readonly Dispatcher _dispatcher;

        public CommandConverter()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public CommandConverter(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ICommandBase cmd)
            {
                var command = new CommandWrapper(cmd, new ViewOperations(_dispatcher));
                return command;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}