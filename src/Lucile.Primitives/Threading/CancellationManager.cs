using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Lucile.Threading
{
    public class CancellationManager
    {
        private readonly object _childrenLocker = new object();

        private readonly object _sourceLocker = new object();
        private CancellationTokenSource _source;

        public CancellationManager()
        {
            this.Children = new Collection<CancellationManager>();
        }

        protected CancellationManager(CancellationManager parent)
            : this()
        {
            this.Parent = parent;
        }

        public CancellationManager Parent { get; }

        public CancellationManager Root
        {
            get
            {
                CancellationManager parent = this;
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        protected Collection<CancellationManager> Children { get; }

        public void Cancel()
        {
            lock (_sourceLocker)
            {
                CancelTree();
            }
        }

        public CancellationManager CreateChildManager()
        {
            var manager = new CancellationManager(this);
            lock (_sourceLocker)
            {
                this.Children.Add(manager);
            }

            return manager;
        }

        public async Task ExecuteAsync(Func<CancellationToken, Task> creator)
        {
            var token = GetToken();

            try
            {
                await creator(token);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        public async Task<TData> ExecuteAsync<TData>(Func<CancellationToken, Task<TData>> creator)
        {
            var token = GetToken();
            var result = default(TData);

            try
            {
                result = await creator(token);
            }
            catch (OperationCanceledException)
            {
                // expected
            }

            return result;
        }

        protected internal CancellationToken GetToken()
        {
            CancellationToken token = CancellationToken.None;

            lock (_sourceLocker)
            {
                CancelTree();

                _source = new CancellationTokenSource();
                token = _source.Token;
            }

            return token;
        }

        private static void DoCancel(CancellationTokenSource source)
        {
            if (source != null && !source.IsCancellationRequested)
            {
                Task.Run(() => source.Cancel());
            }
        }

        private void CancelTree()
        {
            var source = _source;
            _source = null;

            DoCancel(source);

            lock (_childrenLocker)
            {
                foreach (var item in Children)
                {
                    item.Cancel();
                }
            }
        }
    }
}