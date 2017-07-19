using Lucile.Data.Tracking;

namespace Lucile.Data
{
    public interface ITrackingInfoProvider
    {
        TrackingState? GetState(object trackedObject);

        void SetState(object trackedObject, TrackingState? state);
    }
}