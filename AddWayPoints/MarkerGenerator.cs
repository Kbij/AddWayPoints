using System;
using System.Collections.Generic;

namespace AddWayPoints
{
    internal class MarkerSettings
    {
        /// <summary>Afstand tussen markers in km.</summary>
        public double IntervalKm { get; set; }

        /// <summary>Kilometerstand bij de start van dit bestand (bv. km al gereden op vorige dagen).</summary>
        public double OffsetKm { get; set; }

        /// <summary>Geplande afstand per dag in km; 0 = geen dag-markers.</summary>
        public double DayKm { get; set; }

        /// <summary>Naam van de marker; {km} wordt vervangen door de kilometerstand.</summary>
        public string NameFormat { get; set; }
    }

    internal static class MarkerGenerator
    {
        public static List<KmMarker> Generate(IList<RoutePoint> points, MarkerSettings settings)
        {
            var markers = new List<KmMarker>();
            if (points == null || points.Count < 2)
            {
                return markers;
            }

            double total = points[points.Count - 1].Distance;
            double totalKm = settings.OffsetKm + total / 1000.0;

            if (settings.IntervalKm > 0)
            {
                int index = 1;
                long k = (long)Math.Floor(settings.OffsetKm / settings.IntervalKm + 1e-9) + 1;
                for (; ; k++)
                {
                    double labelKm = k * settings.IntervalKm;
                    double distance = (labelKm - settings.OffsetKm) * 1000.0;
                    if (distance > total + 0.001)
                    {
                        break;
                    }
                    var marker = CreateAt(points, distance, ref index);
                    marker.Kind = MarkerKind.Interval;
                    marker.LabelKm = labelKm;
                    marker.Name = FormatName(settings.NameFormat, labelKm);
                    marker.Description = string.Format("Km {0} van {1:0.0}", FormatKm(labelKm), totalKm);
                    markers.Add(marker);
                }
            }

            if (settings.DayKm > 0)
            {
                int index = 1;
                for (int day = 1; ; day++)
                {
                    double distance = day * settings.DayKm * 1000.0;
                    if (distance >= total)
                    {
                        break;
                    }
                    var marker = CreateAt(points, distance, ref index);
                    marker.Kind = MarkerKind.DayEnd;
                    marker.Day = day;
                    marker.LabelKm = settings.OffsetKm + distance / 1000.0;
                    marker.Name = string.Format("Einde dag {0}", day);
                    marker.Description = string.Format("Einde dag {0} na {1:0} km (km {2})",
                        day, distance / 1000.0, FormatKm(marker.LabelKm));
                    markers.Add(marker);
                }
            }

            markers.Sort((a, b) => a.RouteDistance.CompareTo(b.RouteDistance));
            return markers;
        }

        /// <summary>Bepaalt de exacte positie op de route via interpolatie tussen twee trackpunten.</summary>
        static KmMarker CreateAt(IList<RoutePoint> points, double distance, ref int index)
        {
            while (index < points.Count - 1 && points[index].Distance < distance)
            {
                index++;
            }
            var a = points[index - 1];
            var b = points[index];
            double span = b.Distance - a.Distance;
            double t = span > 0 ? (distance - a.Distance) / span : 0;
            t = Math.Max(0, Math.Min(1, t));

            return new KmMarker
            {
                Lat = a.Lat + (b.Lat - a.Lat) * t,
                Lon = a.Lon + (b.Lon - a.Lon) * t,
                RouteDistance = distance
            };
        }

        static string FormatName(string format, double km)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{km}";
            }
            return format.Replace("{km}", FormatKm(km));
        }

        public static string FormatKm(double km)
        {
            return km.ToString("0.##");
        }
    }
}
