using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace AddWayPoints
{
    /// <summary>
    /// Een ingeladen GPX-bestand: de tracks zoals ze in het bestand staan, plus één
    /// doorlopende lijst routepunten met cumulatieve afstand.
    /// </summary>
    internal class GpxRoute
    {
        public const string Gpx11Namespace = "http://www.topografix.com/GPX/1/1";

        /// <summary>Gaten tussen segmenten groter dan dit worden gemeld.</summary>
        const double GapWarningMeters = 50;

        GpxRoute(string fileName, XmlDocument document)
        {
            FileName = fileName;
            Document = document;
            NamespaceUri = string.IsNullOrEmpty(document.DocumentElement.NamespaceURI)
                ? Gpx11Namespace
                : document.DocumentElement.NamespaceURI;
            Ns = new XmlNamespaceManager(document.NameTable);
            Ns.AddNamespace("gpx", NamespaceUri);
            Tracks = new List<Track>();
            Points = new List<RoutePoint>();
            Gaps = new List<KeyValuePair<double, double>>();
        }

        public string FileName { get; private set; }
        public XmlDocument Document { get; private set; }
        public XmlNamespaceManager Ns { get; private set; }
        public string NamespaceUri { get; private set; }
        public List<Track> Tracks { get; private set; }
        public List<RoutePoint> Points { get; private set; }

        /// <summary>Sprongen tussen segmenten: (afstand vanaf start, grootte van de sprong), in meter.</summary>
        public List<KeyValuePair<double, double>> Gaps { get; private set; }

        public double TotalDistance
        {
            get { return Points.Count > 0 ? Points[Points.Count - 1].Distance : 0; }
        }

        public string RouteName
        {
            get
            {
                var named = Tracks.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Name));
                return named != null ? named.Name : Path.GetFileNameWithoutExtension(FileName);
            }
        }

        public static GpxRoute Load(string fileName)
        {
            var document = new XmlDocument();
            document.Load(fileName);
            if (document.DocumentElement == null || document.DocumentElement.LocalName != "gpx")
            {
                throw new InvalidDataException("Dit is geen GPX-bestand.");
            }

            var route = new GpxRoute(fileName, document);
            route.ReadTracks();
            route.BuildRoutePoints();
            return route;
        }

        void ReadTracks()
        {
            foreach (XmlNode trackNode in Document.SelectNodes("//gpx:trk", Ns))
            {
                var track = new Track { Name = InnerTextOf(trackNode, "gpx:name") };
                foreach (XmlNode segmentNode in trackNode.SelectNodes("gpx:trkseg", Ns))
                {
                    var segment = ReadPoints(segmentNode.SelectNodes("gpx:trkpt", Ns));
                    if (segment.Points.Count > 0)
                    {
                        track.Segments.Add(segment);
                    }
                }
                if (track.Segments.Count > 0)
                {
                    Tracks.Add(track);
                }
            }

            // Geen tracks? Probeer dan routes (<rte>), sommige planners exporteren enkel die.
            if (Tracks.Count == 0)
            {
                foreach (XmlNode routeNode in Document.SelectNodes("//gpx:rte", Ns))
                {
                    var segment = ReadPoints(routeNode.SelectNodes("gpx:rtept", Ns));
                    if (segment.Points.Count > 0)
                    {
                        var track = new Track { Name = InnerTextOf(routeNode, "gpx:name") };
                        track.Segments.Add(segment);
                        Tracks.Add(track);
                    }
                }
            }
        }

        static TrackSement ReadPoints(XmlNodeList pointNodes)
        {
            var segment = new TrackSement();
            foreach (XmlNode pointNode in pointNodes)
            {
                var latAttr = pointNode.Attributes["lat"];
                var lonAttr = pointNode.Attributes["lon"];
                if (latAttr == null || lonAttr == null)
                {
                    continue;
                }
                segment.Points.Add(new TrackPoint(
                    GeoMath.ParseInvariant(latAttr.Value),
                    GeoMath.ParseInvariant(lonAttr.Value),
                    pointNode));
            }
            return segment;
        }

        string InnerTextOf(XmlNode node, string xpath)
        {
            var child = node.SelectSingleNode(xpath, Ns);
            return child != null ? child.InnerText.Trim() : null;
        }

        void BuildRoutePoints()
        {
            double distance = 0;
            TrackPoint previous = null;
            foreach (var segment in Tracks.SelectMany(t => t.Segments))
            {
                for (int i = 0; i < segment.Points.Count; i++)
                {
                    var point = segment.Points[i];
                    if (previous != null)
                    {
                        double step = GeoMath.Distance(previous, point);
                        if (i == 0 && step > GapWarningMeters)
                        {
                            Gaps.Add(new KeyValuePair<double, double>(distance, step));
                        }
                        distance += step;
                    }
                    Points.Add(new RoutePoint(point.Lat, point.Lon, distance));
                    previous = point;
                }
            }
        }

        /// <summary>
        /// Schrijft een nieuw GPX-bestand met één track en één segment, in de opgegeven volgorde.
        /// Hoogte/tijd van de punten en bestaande waypoints blijven behouden.
        /// </summary>
        public static GpxRoute SaveSorted(GpxRoute source, IList<TrackSement> orderedSegments, string newFile)
        {
            string ns = source.NamespaceUri;
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlElement gpx = doc.CreateElement("gpx", ns);
            gpx.SetAttribute("version", ns == Gpx11Namespace ? "1.1" : "1.0");
            gpx.SetAttribute("creator", "AddWayPoints");
            doc.AppendChild(gpx);

            XmlElement metadata = doc.CreateElement("metadata", ns);
            XmlElement metaName = doc.CreateElement("name", ns);
            metaName.InnerText = source.RouteName;
            metadata.AppendChild(metaName);
            XmlElement time = doc.CreateElement("time", ns);
            time.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            metadata.AppendChild(time);
            gpx.AppendChild(metadata);

            foreach (XmlNode wpt in source.Document.SelectNodes("//gpx:wpt", source.Ns))
            {
                gpx.AppendChild(doc.ImportNode(wpt, true));
            }

            XmlElement trk = doc.CreateElement("trk", ns);
            XmlElement trkName = doc.CreateElement("name", ns);
            trkName.InnerText = source.RouteName;
            trk.AppendChild(trkName);

            XmlElement trkseg = doc.CreateElement("trkseg", ns);
            foreach (var segment in orderedSegments)
            {
                foreach (var point in segment.Points)
                {
                    XmlElement trkpt;
                    if (point.Source != null && point.Source.LocalName == "trkpt")
                    {
                        trkpt = (XmlElement)doc.ImportNode(point.Source, true);
                    }
                    else
                    {
                        trkpt = doc.CreateElement("trkpt", ns);
                    }
                    trkpt.SetAttribute("lat", GeoMath.FormatCoordinate(point.Lat));
                    trkpt.SetAttribute("lon", GeoMath.FormatCoordinate(point.Lon));
                    trkseg.AppendChild(trkpt);
                }
            }
            trk.AppendChild(trkseg);
            gpx.AppendChild(trk);

            doc.Save(newFile);
            return Load(newFile);
        }
    }
}
