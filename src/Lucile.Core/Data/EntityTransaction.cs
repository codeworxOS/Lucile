using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucile.Data
{
    public class EntityTransaction : IDisposable
    {
        private bool _disposed;

        public EntityTransaction(ModelContext context, IEnumerable<object> protectedEntities)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Entities = ImmutableList.CreateRange<object>(protectedEntities);

            Context = context;
        }

        ~EntityTransaction()
        {
            Dispose(false);
        }

        public ModelContext Context { get; }

        public ImmutableList<object> Entities { get; }

        protected bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Managed Resourcen aufräumen
                    Context.EndTransaction(this);
                }

                // Unmanaged Resourcen aufräumen
                _disposed = true;
            }
        }
    }
}