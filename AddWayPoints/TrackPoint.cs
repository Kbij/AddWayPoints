using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWayPoints
{
    internal class TrackPoint
    {
        public TrackPoint(double lat, double lon)
        {
            Coordinate = new GeoCoordinate(lat, lon);
        }
        public GeoCoordinate Coordinate { get; set; }
    }
}
