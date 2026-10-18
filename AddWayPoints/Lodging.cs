using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AddWayPoints
{
    /// <summary>Een plaats (stad/dorp) langs de route met het aantal logies in de buurt.</summary>
    internal class Town
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PlaceType { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        /// <summary>Afstand vanaf de start tot het dichtstbijzijnde routepunt, in meter.</summary>
        public double RouteDistance { get; set; }

        /// <summary>Afstand van de plaats tot de route, in meter.</summary>
        public double OffRoute { get; set; }

        public int BedAndBreakfast { get; set; }
        public int Hotel { get; set; }
        public int Hostel { get; set; }
        public int Camping { get; set; }
        public int Restaurant { get; set; }
        public int FastFood { get; set; }

        public string FoodSummary
        {
            get
            {
                if (Restaurant == 0 && FastFood == 0) return "geen";
                var parts = new List<string>();
                if (Restaurant > 0) parts.Add(Restaurant + " restaurant");
                if (FastFood > 0) parts.Add(FastFood + " snack");
                return string.Join(", ", parts);
            }
        }

        public int Count(bool includeCamping)
        {
            return BedAndBreakfast + Hotel + Hostel + (includeCamping ? Camping : 0);
        }

        public string Summary(bool includeCamping)
        {
            var parts = new List<string>();
            if (BedAndBreakfast > 0) parts.Add(BedAndBreakfast + " B&B");
            if (Hotel > 0) parts.Add(Hotel + " hotel");
            if (Hostel > 0) parts.Add(Hostel + " hostel");
            if (includeCamping && Camping > 0) parts.Add(Camping + " camping");
            return string.Join(", ", parts);
        }
    }

    /// <summary>Haalt plaatsen, logies en restaurants langs de route op via de OpenStreetMap Overpass API.</summary>
    internal static class LodgingFinder
    {
        static readonly string[] Endpoints =
        {
            "https://overpass-api.de/api/interpreter",
            "https://overpass.private.coffee/api/interpreter",
            "https://overpass.kumi.systems/api/interpreter"
        };

        /// <summary>Plaatsen tot zover van de route worden meegenomen (de planner filtert later strenger).</summary>
        public const double PlaceRadiusMeters = 8000;

        /// <summary>Logies worden toegekend aan de dichtstbijzijnde plaats binnen deze afstand.</summary>
        const double AssignRadiusMeters = 5000;

        /// <summary>Restaurants tellen enkel als ze op wandelafstand van de dorpskern liggen.</summary>
        const double FoodRadiusMeters = 2000;

        /// <summary>Kleine stukken houden de query licht genoeg voor de gratis servers.</summary>
        const double ChunkLengthMeters = 50000;

        const int Rounds = 3;

        static readonly HttpClient Http = CreateClient();

        static readonly string CacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AddWayPoints", "overpass");

        static HttpClient CreateClient()
        {
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(200) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AddWayPoints/1.0 (GPX km-marker tool)");
            return client;
        }

        public static async Task<List<Town>> FindAsync(IList<RoutePoint> route, IProgress<string> progress, CancellationToken token)
        {
            // Route vereenvoudigen tot ongeveer één punt per km, in stukken van ±50 km
            var chunks = new List<List<RoutePoint>>();
            var current = new List<RoutePoint>();
            double lastDistance = double.MinValue;
            double chunkStart = 0;
            foreach (var p in route)
            {
                if (p.Distance - lastDistance < 1000 && p != route[route.Count - 1]) continue;
                current.Add(p);
                lastDistance = p.Distance;
                if (p.Distance - chunkStart >= ChunkLengthMeters)
                {
                    chunks.Add(current);
                    current = new List<RoutePoint> { p };
                    chunkStart = p.Distance;
                }
            }
            if (current.Count > 1 || chunks.Count == 0) chunks.Add(current);

            var towns = new Dictionary<long, Town>();
            var lodgings = new Dictionary<string, KeyValuePair<string, double[]>>();

            for (int i = 0; i < chunks.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                string label = string.Format("Logies en restaurants opzoeken… deel {0}/{1}", i + 1, chunks.Count);
                progress.Report(label);

                string query = BuildQuery(chunks[i]);
                string cacheFile = Path.Combine(CacheDir, Hash(query) + ".xml");
                string xml = ReadCache(cacheFile);
                if (xml == null)
                {
                    xml = await QueryAsync(query, label, progress, token).ConfigureAwait(false);
                    WriteCache(cacheFile, xml);
                    await Task.Delay(1000, token).ConfigureAwait(false);   // vriendelijk voor de gratis servers
                }
                Parse(xml, towns, lodgings);
            }

            progress.Report("Logies en restaurants toekennen aan plaatsen…");
            var result = towns.Values.ToList();
            await Task.Run(() => Assign(result, lodgings.Values, route), token).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Eerst de plaatsen langs de route, daarna enkel logies/restaurants rond die plaatsen.
        /// Veel lichter dan alles in een brede strook langs de route opvragen.
        /// </summary>
        static string BuildQuery(IList<RoutePoint> points)
        {
            var coords = string.Join(",", points.Select(p =>
                p.Lat.ToString("0.####", CultureInfo.InvariantCulture) + "," +
                p.Lon.ToString("0.####", CultureInfo.InvariantCulture)));
            return
                "[out:xml][timeout:120];" +
                "node[\"place\"~\"^(city|town|village)$\"][\"name\"](around:" + (int)PlaceRadiusMeters + "," + coords + ")->.p;" +
                "nwr(around.p:" + (int)AssignRadiusMeters + ")[\"tourism\"~\"^(guest_house|hotel|motel|hostel|apartment|chalet|camp_site)$\"]->.b;" +
                "nwr(around.p:" + (int)FoodRadiusMeters + ")[\"amenity\"~\"^(restaurant|fast_food)$\"]->.f;" +
                "(.p;.b;.f;);out center tags;";
        }

        static async Task<string> QueryAsync(string query, string label, IProgress<string> progress, CancellationToken token)
        {
            var errors = new List<string>();
            for (int round = 1; round <= Rounds; round++)
            {
                foreach (var endpoint in Endpoints)
                {
                    string server = new Uri(endpoint).Host;
                    try
                    {
                        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
                        using (var response = await Http.PostAsync(endpoint, content, token).ConfigureAwait(false))
                        {
                            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (!response.IsSuccessStatusCode)
                            {
                                errors.Add(string.Format("{0}: {1} {2}", server, (int)response.StatusCode, ErrorText(body)));
                                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                                {
                                    // Fout in de query zelf: opnieuw proberen heeft geen zin
                                    throw new InvalidOperationException("Overpass weigert de vraag:\n" + string.Join("\n", errors));
                                }
                                continue;
                            }

                            // Overpass geeft bij een timeout of geheugentekort soms "200 OK" met een
                            // onvolledig antwoord en een <remark> met de fout.
                            var remark = Regex.Match(body, "<remark>(.*?)</remark>", RegexOptions.Singleline);
                            if (remark.Success && remark.Groups[1].Value.Contains("error"))
                            {
                                errors.Add(server + ": " + Shorten(remark.Groups[1].Value.Trim()));
                                continue;
                            }
                            if (!body.Contains("<osm"))
                            {
                                errors.Add(server + ": onverwacht antwoord");
                                continue;
                            }
                            return body;
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                        errors.Add(server + ": geen antwoord binnen de tijd");
                    }
                    catch (Exception ex)
                    {
                        errors.Add(server + ": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                    }
                }

                if (round < Rounds)
                {
                    // Servers druk: even wachten en opnieuw
                    int wait = 15 * round;
                    for (int s = wait; s > 0; s--)
                    {
                        progress.Report(string.Format("{0} — servers druk, nieuwe poging over {1} s", label, s));
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }
                    progress.Report(label);
                }
            }

            // Laatste fout per server tonen
            var lastPerServer = errors
                .GroupBy(e => e.Substring(0, Math.Max(0, e.IndexOf(':'))))
                .Select(g => g.Last());
            throw new InvalidOperationException("Geen enkele Overpass-server kon de vraag beantwoorden:\n" +
                string.Join("\n", lastPerServer));
        }

        /// <summary>Haalt de foutmelding uit de HTML-foutpagina van Overpass.</summary>
        static string ErrorText(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            var m = Regex.Match(html, "<strong[^>]*>Error</strong>:(.*?)</p>", RegexOptions.Singleline);
            string text = m.Success ? m.Groups[1].Value : Regex.Replace(html, "<[^>]+>", " ");
            return Shorten(System.Net.WebUtility.HtmlDecode(Regex.Replace(text, "\\s+", " ").Trim()));
        }

        static string Shorten(string text)
        {
            return text.Length > 140 ? text.Substring(0, 140) + "…" : text;
        }

        static string Hash(string text)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
            }
        }

        static string ReadCache(string file)
        {
            try
            {
                // Logies veranderen niet zo snel: een maand bewaren is prima
                if (File.Exists(file) && File.GetLastWriteTimeUtc(file) > DateTime.UtcNow.AddDays(-30))
                {
                    return File.ReadAllText(file, Encoding.UTF8);
                }
            }
            catch (IOException) { }
            return null;
        }

        static void WriteCache(string file, string xml)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllText(file, xml, Encoding.UTF8);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        /// <summary>Leest Overpass-XML in. Publiek zodat het ook los te testen is.</summary>
        public static void Parse(string xml, Dictionary<long, Town> towns, Dictionary<string, KeyValuePair<string, double[]>> lodgings)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            foreach (XmlElement el in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
            {
                if (el.Name != "node" && el.Name != "way" && el.Name != "relation") continue;

                double lat, lon;
                var center = el["center"];
                if (el.HasAttribute("lat"))
                {
                    lat = GeoMath.ParseInvariant(el.GetAttribute("lat"));
                    lon = GeoMath.ParseInvariant(el.GetAttribute("lon"));
                }
                else if (center != null)
                {
                    lat = GeoMath.ParseInvariant(center.GetAttribute("lat"));
                    lon = GeoMath.ParseInvariant(center.GetAttribute("lon"));
                }
                else continue;

                var tags = new Dictionary<string, string>();
                foreach (XmlElement tag in el.GetElementsByTagName("tag"))
                {
                    tags[tag.GetAttribute("k")] = tag.GetAttribute("v");
                }

                string tourism, amenity, place;
                if (tags.TryGetValue("tourism", out tourism))
                {
                    lodgings[el.Name + el.GetAttribute("id")] = new KeyValuePair<string, double[]>(tourism, new[] { lat, lon });
                }
                else if (tags.TryGetValue("amenity", out amenity) && (amenity == "restaurant" || amenity == "fast_food"))
                {
                    lodgings[el.Name + el.GetAttribute("id")] = new KeyValuePair<string, double[]>(amenity, new[] { lat, lon });
                }
                else if (el.Name == "node" && tags.TryGetValue("place", out place))
                {
                    long id = long.Parse(el.GetAttribute("id"), CultureInfo.InvariantCulture);
                    towns[id] = new Town { Id = id, Name = tags["name"], PlaceType = place, Lat = lat, Lon = lon };
                }
            }
        }

        public static void Assign(List<Town> towns, IEnumerable<KeyValuePair<string, double[]>> lodgings, IList<RoutePoint> route)
        {
            foreach (var lodging in lodgings)
            {
                bool food = lodging.Key == "restaurant" || lodging.Key == "fast_food";
                Town best = null;
                double bestDistance = food ? FoodRadiusMeters : AssignRadiusMeters;
                foreach (var town in towns)
                {
                    // snelle voorselectie (±0,1° ≈ 7-11 km)
                    if (Math.Abs(town.Lat - lodging.Value[0]) > 0.1 || Math.Abs(town.Lon - lodging.Value[1]) > 0.15) continue;
                    double d = GeoMath.Distance(town.Lat, town.Lon, lodging.Value[0], lodging.Value[1]);
                    if (d < bestDistance)
                    {
                        bestDistance = d;
                        best = town;
                    }
                }
                if (best == null) continue;

                switch (lodging.Key)
                {
                    case "hotel":
                    case "motel": best.Hotel++; break;
                    case "hostel": best.Hostel++; break;
                    case "camp_site": best.Camping++; break;
                    case "restaurant": best.Restaurant++; break;
                    case "fast_food": best.FastFood++; break;
                    default: best.BedAndBreakfast++; break;   // guest_house, apartment, chalet
                }
            }

            foreach (var town in towns)
            {
                double best = double.MaxValue;
                foreach (var p in route)
                {
                    if (Math.Abs(p.Lat - town.Lat) > 0.15) continue;
                    double d = GeoMath.Distance(town.Lat, town.Lon, p.Lat, p.Lon);
                    if (d < best)
                    {
                        best = d;
                        town.RouteDistance = p.Distance;
                    }
                }
                town.OffRoute = best;
            }
        }
    }
}
