namespace AddWayPoints
{
    /// <summary>Een plaats uit een KML-bestand (bv. camping of overnachting) en waar die op de route ligt.</summary>
    internal class PlaceMark
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        /// <summary>Afstand vanaf de start van de route tot het dichtstbijzijnde routepunt, in meter.</summary>
        public double RouteDistance { get; set; }

        /// <summary>Hoe ver de plaats naast de route ligt, in meter.</summary>
        public double OffRouteDistance { get; set; }
    }
}
