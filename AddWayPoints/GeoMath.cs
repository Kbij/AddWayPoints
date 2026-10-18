using System;
using System.Globalization;

namespace AddWayPoints
{
    /// <summary>
    /// Eenvoudige geo-berekeningen. Gebruikt dezelfde haversine-formule en aardstraal
    /// als System.Device.Location.GeoCoordinate, zodat afstanden identiek blijven.
    /// </summary>
    internal static class GeoMath
    {
        const double EarthRadiusMeters = 6376500.0;

        public static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public static double Distance(TrackPoint a, TrackPoint b)
        {
            return Distance(a.Lat, a.Lon, b.Lat, b.Lon);
        }

        static double ToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        /// <summary>Parse een coördinaat altijd met punt als decimaalteken (los van de Windows-landinstelling).</summary>
        public static double ParseInvariant(string text)
        {
            return double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static string FormatCoordinate(double value)
        {
            return value.ToString("0.0000000", CultureInfo.InvariantCulture);
        }
    }
}
