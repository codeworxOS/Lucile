using Codeworx.Data.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Codeworx.Synchronization.Data
{
    public interface ISyncableContext
    {
        IQueryable<SyncParameter> GetSyncParameters();
        IQueryable<SyncPeer> GetSyncPeers();
        IQueryable<SyncPartnership> GetSyncPartnerships();
        IQueryable<SyncHistory> GetSyncHistory();

        Task<string> GetPeerName();

        Task<Dictionary<object, byte[]>> GetChangedKeys(EntityMetadata entity, byte[] lastVersion, byte[] lastCommitVersion, ChangeState state);

        Task EnsureSyncEnabled(IEnumerable<EntityMetadata> entities);

        Task<byte[]> GetCurrentVersion();

        Task<byte[]> GetCommitVersion(Guid requestId);
    }
}
