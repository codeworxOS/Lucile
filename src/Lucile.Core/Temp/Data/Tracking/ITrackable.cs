using System.Collections.Generic;

namespace Codeworx.Data.Tracking
{
    public interface ITrackable
    {
        TrackingState? State { get; set; }

        IEnumerable<string> ModifiedProperties { get; }
    }
}
