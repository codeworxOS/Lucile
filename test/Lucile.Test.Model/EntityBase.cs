using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [NotMapped]
        public TrackingState? State
        {
            get; set;
        }
    }
}