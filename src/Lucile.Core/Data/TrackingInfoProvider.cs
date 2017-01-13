using Lucile.Data.Tracking;

namespace Lucile.Data
{
    public class TrackingInfoProvider : ITrackingInfoProvider
    {
        public TrackingState? GetState(object trackedObject)
        {
            var trackabel = trackedObject as ITrackable;
            return trackabel?.State;
        }
    }
}