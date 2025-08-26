using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWayPoints
{
    class PlaceMark
    {
        public PlaceMark()
        {
            TotalRouteDistance = 0;
        }
        public string Name { get; set; }
        public GeoCoordinate Coordinate { get; set; }
        public double TotalRouteDistance { get; set; }
        public RoutePoint ClosestRoutePoint { get; set; }
    }
}
