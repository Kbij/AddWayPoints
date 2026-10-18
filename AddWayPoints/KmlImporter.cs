using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AddWayPoints
{
    /// <summary>Leest plaatsen uit een KML-bestand en bepaalt waar ze op de route liggen.</summary>
    internal static class KmlImporter
    {
        public static List<PlaceMark> Load(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);

            var result = new List<PlaceMark>();
            // Via local-name() werkt het voor elke KML-namespace (2.2, Google, ...).
            foreach (XmlElement placemark in doc.SelectNodes("//*[local-name()='Placemark']"))
            {
                var nameNode = placemark.SelectSingleNode("*[local-name()='name']");
                var coordNode = placemark.SelectSingleNode("*[local-name()='Point']/*[local-name()='coordinates']");
                if (coordNode == null)
                {
                    continue;   // lijnen/polygonen overslaan
                }

                string[] parts = coordNode.InnerText.Trim().Split(',');
                if (parts.Length < 2)
                {
                    continue;
                }

                result.Add(new PlaceMark
                {
                    Name = nameNode != null ? nameNode.InnerText.Trim() : "(naamloos)",
                    Lon = GeoMath.ParseInvariant(parts[0]),
                    Lat = GeoMath.ParseInvariant(parts[1])
                });
            }
            return result;
        }

        /// <summary>Zoekt per plaats het dichtstbijzijnde routepunt en sorteert op afstand langs de route.</summary>
        public static void LocateOnRoute(List<PlaceMark> places, IList<RoutePoint> route)
        {
            foreach (var place in places)
            {
                double best = double.MaxValue;
                RoutePoint bestPoint = null;
                foreach (var point in route)
                {
                    double d = GeoMath.Distance(place.Lat, place.Lon, point.Lat, point.Lon);
                    if (d < best)
                    {
                        best = d;
                        bestPoint = point;
                    }
                }
                if (bestPoint != null)
                {
                    place.RouteDistance = bestPoint.Distance;
                    place.OffRouteDistance = best;
                }
            }

            var sorted = places.OrderBy(p => p.RouteDistance).ToList();
            places.Clear();
            places.AddRange(sorted);
        }
    }
}
