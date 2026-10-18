using System.Collections.Generic;
using System.Linq;

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

        public double Length
        {
            get { return Segments.Sum(s => s.Length); }
        }
    }
}
