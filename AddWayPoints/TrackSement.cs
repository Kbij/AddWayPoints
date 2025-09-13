using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWayPoints
{
    internal class TrackSement
    {
        public TrackSement()
        {
            Points = new List<TrackPoint>();
        }

        public List<TrackPoint> Points { get; set; }

    }
}
