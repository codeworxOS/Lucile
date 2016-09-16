using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeworx.Threading
{
    public class CancellationManager
    {
        public CancellationManager Parent { get; private set; }

        public CancellationManager Root
        {
            get
            {
                CancellationManager parent = this;
                while (parent.Parent != null) {
                    parent = parent.Parent;
                }
                return parent;
            }
        }

        protected Collection<CancellationManager> Children { get; private set; }

        protected CancellationManager(CancellationManager parent)
            : this()
        {
            this.Parent = parent;
        }

        public CancellationManager()
        {
            this.Children = new Collection<CancellationManager>();
        }

        private CancellationTokenSource source;
        private object sourceLocker = new object();
        private object childrenLocker = new object();

        public async Task Execute(Func<CancellationToken, Task> creator)
        {
            var token = GetToken();

            try {
                await creator(token);
            } catch (OperationCanceledException) {
                // shit happens!!!
            }
        }

        public async Task<TData> Execute<TData>(Func<CancellationToken, Task<TData>> creator)
        {
            var token = GetToken();
            var result = default(TData);

            try {
                result = await creator(token);
            } catch (OperationCanceledException) {
                // shit happens!!!
            }

            return result;
        }

        public void Cancel()
        {
            lock (sourceLocker) {
                DoCancel();

                source = null;
            }
        }

        private void DoCancel()
        {
            if (source != null && !source.IsCancellationRequested) {
                source.Cancel();
            }

            lock (childrenLocker) {
                foreach (var item in Children) {
                    item.Cancel();
                }
            }
        }

        public CancellationManager CreateChildManager()
        {
            var manager = new CancellationManager(this);
            lock (sourceLocker) {
                this.Children.Add(manager);
            }
            return manager;
        }

        protected internal CancellationToken GetToken()
        {
            CancellationToken token = CancellationToken.None;

            lock (sourceLocker) {
                DoCancel();

                source = new CancellationTokenSource();
                token = source.Token;
            }

            return token;
        }
    }
}
