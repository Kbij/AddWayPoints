using System.Collections.Generic;
using System.Linq;

namespace AddWayPoints
{
    internal class TrackSement
    {
        public TrackSement()
        {
            Points = new List<TrackPoint>();
        }

        public List<TrackPoint> Points { get; set; }

        public TrackPoint First { get { return Points[0]; } }
        public TrackPoint Last { get { return Points[Points.Count - 1]; } }

        public double Length
        {
            get
            {
                double length = 0;
                for (int i = 1; i < Points.Count; i++)
                {
                    length += GeoMath.Distance(Points[i - 1], Points[i]);
                }
                return length;
            }
        }

        public TrackSement Reversed()
        {
            var reversed = new TrackSement();
            reversed.Points.AddRange(Enumerable.Reverse(Points));
            return reversed;
        }
    }
}
