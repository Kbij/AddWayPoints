using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWayPoints
{
    internal class Track
    {
        public Track()
        {
            Segments = new List<TrackSement>();
        }
        public string Name { get; set; }
        public List<TrackSement> Segments { get; set; }
    }
}
