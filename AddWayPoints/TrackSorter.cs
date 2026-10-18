using System;
using System.Collections.Generic;
using System.Linq;

namespace AddWayPoints
{
    /// <summary>
    /// Zet losse tracksegmenten (bv. een JOSM-export) in de juiste volgorde door
    /// het einde van elk segment aan het begin van het volgende te koppelen.
    /// </summary>
    internal static class TrackSorter
    {
        public static List<TrackSement> Sort(IEnumerable<Track> tracks, double toleranceMeters, Action<string> log)
        {
            var all = tracks.SelectMany(t => t.Segments).Where(s => s.Points.Count > 0).ToList();
            if (all.Count <= 1)
            {
                return all;
            }

            // Startsegment: een segment waar geen ander segment op aansluit.
            TrackSement start = all.FirstOrDefault(s =>
                !all.Any(o => o != s && GeoMath.Distance(o.Last, s.First) < toleranceMeters)) ?? all[0];

            var result = new List<TrackSement> { start };
            var remaining = new List<TrackSement>(all);
            remaining.Remove(start);
            var current = start;
            int bridged = 0;

            while (remaining.Count > 0)
            {
                TrackSement original = remaining.FirstOrDefault(s =>
                    GeoMath.Distance(current.Last, s.First) < toleranceMeters);
                TrackSement next = original;

                if (original == null)
                {
                    // Niets sluit netjes aan: neem het dichtstbijzijnde segment (eventueel omgekeerd).
                    double bestDistance = double.MaxValue;
                    bool reverse = false;
                    foreach (var candidate in remaining)
                    {
                        double toStart = GeoMath.Distance(current.Last, candidate.First);
                        double toEnd = GeoMath.Distance(current.Last, candidate.Last);
                        if (toStart < bestDistance) { bestDistance = toStart; original = candidate; reverse = false; }
                        if (toEnd < bestDistance) { bestDistance = toEnd; original = candidate; reverse = true; }
                    }
                    next = reverse ? original.Reversed() : original;
                    bridged++;
                    log(string.Format("  Let op: geen aansluitend segment, dichtstbijzijnde op {0:N0} m gebruikt{1}.",
                        bestDistance, reverse ? " (omgekeerd)" : ""));
                }

                remaining.Remove(original);
                result.Add(next);
                current = next;
            }

            log(string.Format("{0} segmenten gesorteerd{1}.", result.Count,
                bridged > 0 ? string.Format(", {0} keer overbrugd", bridged) : ""));
            return result;
        }
    }
}
