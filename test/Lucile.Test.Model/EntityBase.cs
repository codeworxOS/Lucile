using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Tracking;

namespace Lucile.Test.Model
{
    public class EntityBase : ITrackable
    {
        public IEnumerable<string> ModifiedProperties
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public TrackingState? State
        {
            get; set;
        }
    }
}