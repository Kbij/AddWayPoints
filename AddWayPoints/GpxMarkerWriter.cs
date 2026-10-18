using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace AddWayPoints
{
    /// <summary>Schrijft km-markers als GPX-waypoints, in een apart bestand of in de route zelf.</summary>
    internal static class GpxMarkerWriter
    {
        /// <summary>
        /// Waarde van &lt;type&gt; bij gegenereerde markers. Zo kunnen we ze later terugvinden
        /// en vervangen zonder je eigen waypoints aan te raken.
        /// </summary>
        public const string MarkerType = "AddWayPoints-km";

        public const string DaySymbol = "Lodging";

        public static string SeparateFileName(string gpxFile)
        {
            return Path.Combine(Path.GetDirectoryName(gpxFile),
                Path.GetFileNameWithoutExtension(gpxFile) + "-km.gpx");
        }

        /// <summary>Apart GPX-bestand met enkel de markers: in je app in te laden als eigen laag.</summary>
        public static string WriteSeparateFile(GpxRoute route, IList<KmMarker> markers, string symbol)
        {
            string ns = GpxRoute.Gpx11Namespace;
            string fileName = SeparateFileName(route.FileName);

            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlElement gpx = doc.CreateElement("gpx", ns);
            gpx.SetAttribute("version", "1.1");
            gpx.SetAttribute("creator", "AddWayPoints");
            doc.AppendChild(gpx);

            XmlElement metadata = doc.CreateElement("metadata", ns);
            XmlElement name = doc.CreateElement("name", ns);
            name.InnerText = route.RouteName + " - km-markers";
            metadata.AppendChild(name);
            XmlElement time = doc.CreateElement("time", ns);
            time.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            metadata.AppendChild(time);
            gpx.AppendChild(metadata);

            foreach (var marker in markers)
            {
                gpx.AppendChild(CreateWaypoint(doc, ns, marker, symbol));
            }

            doc.Save(fileName);
            return fileName;
        }

        /// <summary>
        /// Voegt de markers toe aan het GPX-bestand zelf (na een backup). Eerder gegenereerde
        /// markers worden altijd vervangen; andere waypoints enkel als removeOtherWaypoints.
        /// </summary>
        public static string WriteIntoRoute(GpxRoute route, IList<KmMarker> markers, string symbol,
            bool removeOtherWaypoints, out int removed)
        {
            XmlDocument doc = route.Document;
            removed = 0;

            foreach (XmlNode wpt in doc.SelectNodes("//gpx:wpt", route.Ns).Cast<XmlNode>().ToList())
            {
                var typeNode = wpt.SelectSingleNode("gpx:type", route.Ns);
                bool generated = typeNode != null && typeNode.InnerText == MarkerType;
                if (generated || removeOtherWaypoints)
                {
                    wpt.ParentNode.RemoveChild(wpt);
                    removed++;
                }
            }

            // GPX-volgorde: metadata, wpt*, rte*, trk*, extensions
            XmlElement root = doc.DocumentElement;
            XmlNode insertAfter = doc.SelectNodes("/gpx:gpx/gpx:wpt", route.Ns).Cast<XmlNode>().LastOrDefault()
                                  ?? root.SelectSingleNode("gpx:metadata", route.Ns);

            foreach (var marker in markers)
            {
                XmlElement wpt = CreateWaypoint(doc, route.NamespaceUri, marker, symbol);
                if (insertAfter == null)
                {
                    root.PrependChild(wpt);
                }
                else
                {
                    root.InsertAfter(wpt, insertAfter);
                }
                insertAfter = wpt;
            }

            string backup = MakeBackup(route.FileName);
            doc.Save(route.FileName);
            return backup;
        }

        static XmlElement CreateWaypoint(XmlDocument doc, string ns, KmMarker marker, string symbol)
        {
            XmlElement wpt = doc.CreateElement("wpt", ns);
            wpt.SetAttribute("lat", GeoMath.FormatCoordinate(marker.Lat));
            wpt.SetAttribute("lon", GeoMath.FormatCoordinate(marker.Lon));

            // Volgorde van de child-elementen volgens het GPX-schema
            AppendText(doc, wpt, ns, "name", marker.Name);
            AppendText(doc, wpt, ns, "desc", marker.Description);
            string sym = marker.Kind == MarkerKind.DayEnd ? DaySymbol : symbol;
            if (!string.IsNullOrWhiteSpace(sym))
            {
                AppendText(doc, wpt, ns, "sym", sym.Trim());
            }
            AppendText(doc, wpt, ns, "type", MarkerType);
            return wpt;
        }

        static void AppendText(XmlDocument doc, XmlElement parent, string ns, string name, string text)
        {
            XmlElement element = doc.CreateElement(name, ns);
            element.InnerText = text ?? "";
            parent.AppendChild(element);
        }

        static string MakeBackup(string fileName)
        {
            string backupDir = Path.Combine(Path.GetDirectoryName(fileName), "backup");
            Directory.CreateDirectory(backupDir);
            int index = 0;
            string backupFile;
            do
            {
                backupFile = Path.Combine(backupDir, Path.GetFileName(fileName) + "." + index);
                index++;
            } while (File.Exists(backupFile));

            File.Copy(fileName, backupFile);
            return backupFile;
        }
    }
}
