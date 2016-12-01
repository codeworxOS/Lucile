using System.Collections.Generic;

namespace Lucile.Data.Tracking
{
    public interface ITrackable
    {
        IEnumerable<string> ModifiedProperties { get; }

        TrackingState? State { get; set; }
    }
}