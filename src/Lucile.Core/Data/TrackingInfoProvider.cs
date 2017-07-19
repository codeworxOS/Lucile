using Lucile.Data.Tracking;

namespace Lucile.Data
{
    public class TrackingInfoProvider : ITrackingInfoProvider
    {
        public TrackingState? GetState(object trackedObject)
        {
            var trackable = trackedObject as ITrackable;
            return trackable?.State;
        }

        public void SetState(object trackedObject, TrackingState? state)
        {
            var trackable = trackedObject as ITrackable;
            if (trackable != null)
            {
                trackable.State = state;
            }
        }
    }
}