using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWayPoints
{
    class RoutePoint
    {
        public GeoCoordinate Coordinate { get; set; }
        public double RouteDistance { get; set; }
    }
}
