using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AddWayPoints
{
    public partial class MainForm : Form
    {
        XmlDocument mGpxDocument;
        XmlNamespaceManager mNsMgr;
        List<RoutePoint> mRoute;
        const string XMLBEGIN = @"<?xml version='1.0' encoding='UTF-8'?>
<gpx version=""1.1"" creator=""JOSM GPX export"" xmlns=""http://www.topografix.com/GPX/1/1""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
    xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"">
  <metadata>
  </metadata>
  <trk>
<trkseg>
";

        const string XMLEND = @"  </trkseg>
</trk>
</gpx>";
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            mRoute = new List<RoutePoint>();
            btnWriteWaypoints.Enabled = false;
            dlgOpenFile.FileName = "*.gpx";
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    AddLog(string.Format("File selected: {0}", dlgOpenFile.FileName));
                    mGpxDocument = new XmlDocument();
                    mGpxDocument.Load(dlgOpenFile.FileName);
                    mNsMgr = new XmlNamespaceManager(mGpxDocument.NameTable);
                    mNsMgr.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1");

                    if (cbxSortTraks.Checked)
                    {
                        SortTracks();
                    }
                    double totalDistance = 0;
                    double prevLat = -1;
                    double prevLon = -1;

                    foreach (XmlNode track in mGpxDocument.SelectNodes("//gpx:trk", mNsMgr))
                    {
                        var nameNode = track.SelectSingleNode("gpx:name", mNsMgr);
                        if (nameNode != null)
                        {
                            AddLog($"Track: {nameNode.InnerText}");
                        }

                        XmlNodeList segmentNodes = track.SelectNodes("gpx:trkseg", mNsMgr);
                        double trackDistance = 0;

                        foreach (XmlNode segment in segmentNodes)
                        {
                            foreach (XmlNode pt in segment.SelectNodes("gpx:trkpt", mNsMgr))
                            {
                                double currentLat = Convert.ToDouble(pt.Attributes["lat"].Value);
                                double currentLon = Convert.ToDouble(pt.Attributes["lon"].Value);

                                if (prevLat != -1 && prevLon != -1)
                                {
                                    var prevCoord = new GeoCoordinate(prevLat, prevLon);
                                    var currentCoord = new GeoCoordinate(currentLat, currentLon);

                                    trackDistance += currentCoord.GetDistanceTo(prevCoord);

                                    var routePoint = new RoutePoint();
                                    routePoint.Coordinate = currentCoord;
                                    routePoint.RouteDistance = (int)Math.Truncate(trackDistance / 1000);
                                    mRoute.Add(routePoint);
                                }
                                prevLat = currentLat;
                                prevLon = currentLon;
                            }
                        }

                        AddLog(string.Format("Distance for current track: {0:N1} km", trackDistance / 1000));
                        AddLog("===========");
                        totalDistance += trackDistance;
                    }

                    AddLog(string.Format("Total distance: {0:N1} km", totalDistance / 1000));
                    btnWriteWaypoints.Enabled = true;
                }
                catch (Exception ex)
                {
                    AddLog(string.Format("Exception reading file: {0}", ex.Message));
                }

            }
        }

        private void AddLog(string log)
        {
            edtLog.AppendText($"{log}\r\n");
        }

        private void BtnWriteWaypoints_Click(object sender, EventArgs e)
        {
            XmlNodeList wpNodes = mGpxDocument.SelectNodes("//gpx:wpt", mNsMgr);

            if (btnRemove.Checked)
            {
                foreach (XmlNode wp in wpNodes)
                {
                    wp.RemoveAll();
                    wp.ParentNode.RemoveChild(wp);
                }
            }
            double offSet = Convert.ToDouble(edtOffset.Text);
            double wantedWpdistance = Convert.ToDouble(edtDistance.Text);
            int wpIndex = (int)Math.Truncate(offSet / wantedWpdistance);
            double prevLat = -1;
            double prevLon = -1;
            double totalDistance = 0;

            XmlNode startWpNode = mGpxDocument.SelectSingleNode("//gpx:metadata", mNsMgr);
            if (startWpNode == null)
            {
                startWpNode = mGpxDocument.SelectSingleNode("//gpx:gpx", mNsMgr);
            }

            foreach (XmlNode track in mGpxDocument.SelectNodes("//gpx:trk", mNsMgr))
            {
                XmlNodeList segmentNodes = track.SelectNodes("gpx:trkseg", mNsMgr);
                foreach (XmlNode segment in segmentNodes)
                {
                    foreach (XmlNode pt in segment.SelectNodes("gpx:trkpt", mNsMgr))
                    {
                        double currentLat = Convert.ToDouble(pt.Attributes["lat"].Value);
                        double currentLon = Convert.ToDouble(pt.Attributes["lon"].Value);

                        if (prevLat != -1 && prevLon != -1)
                        {
                            var prevCoord = new GeoCoordinate(prevLat, prevLon);
                            var currentCoord = new GeoCoordinate(currentLat, currentLon);

                            totalDistance += currentCoord.GetDistanceTo(prevCoord);

                            var routePoint = new RoutePoint();
                            routePoint.Coordinate = currentCoord;
                            routePoint.RouteDistance = (int)Math.Truncate(totalDistance / 1000);
                            mRoute.Add(routePoint);
                        }

                        int currentIndex = (int)Math.Truncate((offSet + (totalDistance / 1000)) / wantedWpdistance);
                        if (currentIndex > wpIndex)
                        {
                            wpIndex = currentIndex;
                            XmlNode wpNode = mGpxDocument.CreateElement("wpt", "http://www.topografix.com/GPX/1/1");
                            XmlAttribute latAttr = mGpxDocument.CreateAttribute("lat");
                            latAttr.Value = currentLat.ToString();
                            wpNode.Attributes.Append(latAttr);

                            XmlAttribute lonAttr = mGpxDocument.CreateAttribute("lon");
                            lonAttr.Value = currentLon.ToString();
                            wpNode.Attributes.Append(lonAttr);

                            XmlNode nameNode = mGpxDocument.CreateElement("name", "http://www.topografix.com/GPX/1/1");
                            nameNode.InnerText = string.Format("{0}", wpIndex * wantedWpdistance);
                            wpNode.AppendChild(nameNode);

                            startWpNode.ParentNode.InsertAfter(wpNode, startWpNode);

                            startWpNode = wpNode;
                        }

                        prevLat = currentLat;
                        prevLon = currentLon;
                    }
                }
            }


            Directory.CreateDirectory(Path.GetDirectoryName(dlgOpenFile.FileName) + @"\backup");
            int index = 0;
            string backupFile = "";
            do
            {
                backupFile = Path.GetDirectoryName(dlgOpenFile.FileName) + @"\backup\" + Path.GetFileName(dlgOpenFile.FileName) + "." + index.ToString();
                index++;
            } while (File.Exists(backupFile));

                
            File.Copy(dlgOpenFile.FileName, backupFile);
            mGpxDocument.Save(dlgOpenFile.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<PlaceMark> placeMarkList = new List<PlaceMark>();
            dlgOpenFile.FileName = "*.kml";
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    AddLog(string.Format("File selected: {0}", dlgOpenFile.FileName));
                    mGpxDocument = new XmlDocument();
                    mGpxDocument.Load(dlgOpenFile.FileName);
                    mNsMgr = new XmlNamespaceManager(mGpxDocument.NameTable);
                    mNsMgr.AddNamespace("kml", "http://www.opengis.net/kml/2.2");
                    XmlNodeList placeMarks = mGpxDocument.SelectNodes("//kml:Placemark", mNsMgr);
                    AddLog($"Placemarks found: {placeMarks.Count}");
                    foreach (XmlElement placeMark in placeMarks)
                    {
                        string name = "";
                        string coordinates = "";
                        foreach (XmlElement sub in placeMark.ChildNodes)
                        {
                            if (sub.Name == "name")
                            {
                                name = sub.InnerText;
                            }
                            if (sub.Name == "Point")
                            {
                                foreach (XmlElement subsub in sub.ChildNodes)
                                {
                                    if (subsub.Name == "coordinates")
                                    {
                                        coordinates = subsub.InnerText;
                                        coordinates = Regex.Replace(coordinates, @"\t|\n|\r", "").Trim();
                                    }
                                }
                            }
                        }
                        //var name = placeMark.. Item("name").InnerText;
                        AddLog($"place: {name}, coodinates:{coordinates}");
                        string[] parts = coordinates.Split(',');
                        double placeLong = Convert.ToDouble(parts[0]);
                        double placeLat = Convert.ToDouble(parts[1]);

                        PlaceMark kmlPlaceMark = new PlaceMark();
                        kmlPlaceMark.Coordinate = new GeoCoordinate(placeLat, placeLong);
                        kmlPlaceMark.Name = name;
                        placeMarkList.Add(kmlPlaceMark);
                    }

                    foreach (var placeMark in placeMarkList)
                    {
                        foreach (var routePoint in mRoute)
                        {
                            if (placeMark.ClosestRoutePoint == null)
                            {
                                placeMark.ClosestRoutePoint = routePoint;
                            }
                            else
                            {
                                double previousDistance = Math.Abs(placeMark.Coordinate.GetDistanceTo(placeMark.ClosestRoutePoint.Coordinate));
                                double currentDistance = Math.Abs(placeMark.Coordinate.GetDistanceTo(routePoint.Coordinate));
                                
                                if (currentDistance < previousDistance)
                                {
                                    placeMark.ClosestRoutePoint.Coordinate = routePoint.Coordinate;
                                    placeMark.TotalRouteDistance = routePoint.RouteDistance;
                                }
                            }
                        }
                    }
                    double previousRouteDistance = 0;
                    foreach (var placeMark in placeMarkList)
                    {
                        AddLog($"{placeMark.Name}: {placeMark.TotalRouteDistance - previousRouteDistance}");
                        previousRouteDistance = placeMark.TotalRouteDistance;
                    }
                }
                catch (Exception ex)
                {
                    AddLog(string.Format("Exception reading file: {0}", ex.Message));
                }
            }
        }
        void SortTracks()
        {
            List<Track> tracks = new List<Track>();
            foreach (XmlNode trackNode in mGpxDocument.SelectNodes("//gpx:trk", mNsMgr))
            {
                Track track = new Track();
                var nameNode = trackNode.SelectSingleNode("gpx:name", mNsMgr);
                if (nameNode != null)
                {
                    AddLog($"Track: {nameNode.InnerText}");
                    track.Name = nameNode.InnerText;
                }

                XmlNodeList segmentNodes = trackNode.SelectNodes("gpx:trkseg", mNsMgr);
                foreach (XmlNode segmentNode in segmentNodes)
                {
                    TrackSement segment = new TrackSement();
                    foreach (XmlNode pointNode in segmentNode.SelectNodes("gpx:trkpt", mNsMgr))
                    {
                        double currentLat = Convert.ToDouble(pointNode.Attributes["lat"].Value);
                        double currentLon = Convert.ToDouble(pointNode.Attributes["lon"].Value);
                        segment.Points.Add(new TrackPoint(currentLat, currentLon));
                    }
                    track.Segments.Add(segment);
                }
                tracks.Add(track);
            }

            if (tracks.Count > 0 && tracks[0].Segments.Count > 0)
            {
                TrackSement startSegment = tracks[0].Segments[0];
                foreach (var track in tracks)
                {
                    foreach (var segment in track.Segments)
                    {
                        startSegment = FindPrevious(tracks, startSegment, 300);
                    }
                }
                AddLog($"{startSegment.Points[0].Coordinate.ToString()}");
                string newFile = Path.Combine(Path.GetDirectoryName(dlgOpenFile.FileName), Path.GetFileNameWithoutExtension(dlgOpenFile.FileName) + "-sorted.gpx");
                using (StreamWriter writer = new StreamWriter(newFile, false))
                {
                    TrackSement currentSegment = startSegment;
                    writer.Write(XMLBEGIN);
                    WriteSegment(writer, currentSegment);
                    currentSegment = FindNext(tracks, startSegment, 300);
                    while (currentSegment != null)
                    {
                        WriteSegment(writer, currentSegment);
                        currentSegment = FindNext(tracks, currentSegment, 300);
                    }


                    writer.Write(XMLEND);
                }
            }
        }

        TrackSement FindPrevious(List<Track> tracks, TrackSement startSegment, double allowedDistanceMeter)
        {
            foreach (var track in tracks)
            {
                foreach (var segment in track.Segments)
                {
                    if (startSegment.Points[0].Coordinate.GetDistanceTo(segment.Points.Last().Coordinate) < allowedDistanceMeter)
                    {
                        return segment;
                    }
                }
            }
            return startSegment;
        }

        TrackSement FindNext(List<Track> tracks, TrackSement currentSegment, double allowedDistanceMeter)
        {
            foreach (var track in tracks)
            {
                foreach (var segment in track.Segments)
                {
                    if (currentSegment.Points.Last().Coordinate.GetDistanceTo(segment.Points[0].Coordinate) < allowedDistanceMeter)
                    {
                        return segment;
                    }
                }
            }
            return null;
        }
        void WriteSegment(StreamWriter writer, TrackSement segment)
        {
            foreach(var point in segment.Points)
            {
                writer.WriteLine($"     <trkpt lat = \"{point.Coordinate.Latitude}\" lon = \"{point.Coordinate.Longitude}\">");
                writer.WriteLine($"     </trkpt >");
            }
        }
    }
}
