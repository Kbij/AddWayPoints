using System;
using System.Collections.Generic;
using System.Linq;

namespace AddWayPoints
{
    /// <summary>Eén etappe: van de vorige overnachting tot deze (Town == null = finish).</summary>
    internal class Stage
    {
        public int Day { get; set; }
        public Town Town { get; set; }
        public double StartDistance { get; set; }
        public double EndDistance { get; set; }

        public double Length
        {
            get { return EndDistance - StartDistance; }
        }

        public string Name
        {
            get { return Town != null ? Town.Name : "Finish"; }
        }
    }

    internal class PlanSettings
    {
        public double DayKm { get; set; }
        public double MaxOffRouteKm { get; set; }
        public int MinLodgings { get; set; }

        /// <summary>Minimum aantal restaurants (snackbars tellen half mee).</summary>
        public int MinRestaurants { get; set; }
        public bool IncludeCamping { get; set; }
    }

    /// <summary>
    /// Kiest overnachtingsplaatsen zodat elke dag zo dicht mogelijk bij de gewenste afstand ligt.
    /// Kortste-pad (dynamisch programmeren) over alle geschikte plaatsen, gesorteerd langs de route.
    /// </summary>
    internal static class StagePlanner
    {
        /// <summary>Binnen deze marge (±) rond de dagafstand is een etappe "normaal".</summary>
        public const double Tolerance = 0.25;

        public static List<Town> Candidates(IEnumerable<Town> towns, double totalDistance, PlanSettings s)
        {
            return towns
                .Where(t => t.OffRoute <= s.MaxOffRouteKm * 1000
                         && t.Count(s.IncludeCamping) >= s.MinLodgings
                         && FoodScore(t) >= s.MinRestaurants
                         && t.RouteDistance > 2000
                         && t.RouteDistance < totalDistance - 2000)
                .OrderBy(t => t.RouteDistance)
                .ToList();
        }

        public static List<Stage> Plan(IEnumerable<Town> towns, double totalDistance, PlanSettings s)
        {
            var candidates = Candidates(towns, totalDistance, s);
            double target = s.DayKm * 1000;

            // Knopen: 0 = start, 1..n = plaatsen, n+1 = finish
            int n = candidates.Count;
            var distance = new double[n + 2];
            distance[0] = 0;
            for (int i = 0; i < n; i++) distance[i + 1] = candidates[i].RouteDistance;
            distance[n + 1] = totalDistance;

            var cost = new double[n + 2];
            var previous = new int[n + 2];
            for (int j = 1; j <= n + 1; j++)
            {
                cost[j] = double.MaxValue;
                bool finish = j == n + 1;
                for (int i = j - 1; i >= 0; i--)
                {
                    if (cost[i] == double.MaxValue) continue;
                    double length = distance[j] - distance[i];
                    if (length < 1000) continue;
                    double c = cost[i] + DayCost(length, target, finish) + (finish ? 0 : TownCost(candidates[j - 1], s));
                    if (c < cost[j])
                    {
                        cost[j] = c;
                        previous[j] = i;
                    }
                }
            }

            // Terug naar de start volgen
            var nodes = new List<int>();
            for (int k = n + 1; k > 0; k = previous[k]) nodes.Add(k);
            nodes.Reverse();

            var stages = new List<Stage>();
            double start = 0;
            int day = 1;
            foreach (int node in nodes)
            {
                var stage = new Stage
                {
                    Day = day++,
                    Town = node <= n ? candidates[node - 1] : null,
                    StartDistance = start,
                    EndDistance = distance[node]
                };
                stages.Add(stage);
                start = stage.EndDistance;
            }
            return stages;
        }

        static double DayCost(double length, double target, bool lastDay)
        {
            double deviation = (length - target) / target;
            // Een kortere laatste dag is geen probleem
            if (lastDay && deviation < 0) deviation *= 0.3;

            double c = 0.05 + deviation * deviation;     // vaste kost per dag: niet onnodig opsplitsen
            double excess = Math.Abs(deviation) - Tolerance;
            if (excess > 0) c += 8 * excess * excess;    // buiten de marge wordt het snel duur
            return c;
        }

        static double TownCost(Town town, PlanSettings s)
        {
            // Liever plaatsen met meer keuze (slapen én eten) en dicht bij de route
            return -0.03 * Math.Log(1 + town.Count(s.IncludeCamping))
                   - 0.03 * Math.Log(1 + FoodScore(town))
                   + 0.01 * town.OffRoute / 1000;
        }

        /// <summary>Restaurants tellen volledig, snackbars half.</summary>
        public static double FoodScore(Town town)
        {
            return town.Restaurant + 0.5 * town.FastFood;
        }

        public static bool IsOutsideTolerance(Stage stage, double dayKm, bool lastDay)
        {
            double deviation = (stage.Length / 1000 - dayKm) / dayKm;
            return deviation > Tolerance || (!lastDay && deviation < -Tolerance);
        }
    }
}
