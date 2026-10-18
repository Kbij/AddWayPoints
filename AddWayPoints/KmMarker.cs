namespace AddWayPoints
{
    internal enum MarkerKind
    {
        Interval,
        DayEnd
    }

    /// <summary>Een gegenereerde marker (waypoint) op de route.</summary>
    internal class KmMarker
    {
        public MarkerKind Kind { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        /// <summary>Kilometerstand inclusief offset (wat op de marker staat).</summary>
        public double LabelKm { get; set; }

        /// <summary>Afstand vanaf de start van dit GPX-bestand, in meter.</summary>
        public double RouteDistance { get; set; }

        /// <summary>Dagnummer voor dag-markers.</summary>
        public int Day { get; set; }
    }
}
