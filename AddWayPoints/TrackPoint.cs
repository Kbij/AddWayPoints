using System.Xml;

namespace AddWayPoints
{
    /// <summary>Een punt uit een GPX-tracksegment.</summary>
    internal class TrackPoint
    {
        public TrackPoint(double lat, double lon, XmlNode source = null)
        {
            Lat = lat;
            Lon = lon;
            Source = source;
        }

        public double Lat { get; private set; }
        public double Lon { get; private set; }

        /// <summary>Originele XML-node (zodat hoogte, tijd, ... behouden blijven bij sorteren).</summary>
        public XmlNode Source { get; private set; }
    }
}
