namespace AddWayPoints
{
    /// <summary>Een punt op de (samengevoegde) route met de afgelegde afstand vanaf de start.</summary>
    internal class RoutePoint
    {
        public RoutePoint(double lat, double lon, double distance)
        {
            Lat = lat;
            Lon = lon;
            Distance = distance;
        }

        public double Lat { get; private set; }
        public double Lon { get; private set; }

        /// <summary>Afstand vanaf de start van de route, in meter.</summary>
        public double Distance { get; private set; }
    }
}
