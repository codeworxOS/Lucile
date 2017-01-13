using Lucile.Data.Tracking;

namespace Lucile.Data
{
    public interface ITrackingInfoProvider
    {
        TrackingState? GetState(object trackedObject);
    }
}