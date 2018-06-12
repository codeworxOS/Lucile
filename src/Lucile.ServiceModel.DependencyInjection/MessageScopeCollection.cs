using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lucile.ServiceModel.DependencyInjection
{
    public class MessageScopeCollection : IDisposable
    {
        private readonly List<IMessageScope> _messageScopes;

        private bool _disposedValue = false;

        public MessageScopeCollection()
        {
            _messageScopes = new List<IMessageScope>();
            MessageScopes = new ReadOnlyCollection<IMessageScope>(_messageScopes);
        }

        public IReadOnlyCollection<IMessageScope> MessageScopes { get; }

        public void Add(IMessageScope scope)
        {
            _messageScopes.Add(scope);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Register(IServiceProvider provider)
        {
            foreach (var item in MessageScopes)
            {
                item.Register(provider);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _messageScopes.ForEach(p => p.Dispose());
                }

                _disposedValue = true;
            }
        }
    }
}