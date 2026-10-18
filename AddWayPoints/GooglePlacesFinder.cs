using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace AddWayPoints
{
    /// <summary>
    /// Zoekt logies en restaurants langs de route via de Google Places API (New), Nearby Search.
    /// Elke ~5 km langs de route: één zoekopdracht naar logies en één naar restaurants
    /// (max. 20 resultaten per zoekopdracht). Resultaten worden gegroepeerd per gemeente/plaats.
    /// </summary>
    internal static class GooglePlacesFinder
    {
        const string Endpoint = "https://places.googleapis.com/v1/places:searchNearby";

        // Enkel velden uit de "Pro"-SKU vragen (geen rating/openingsuren = duurdere Enterprise-SKU)
        const string FieldMask = "places.id,places.primaryType,places.types,places.location,places.addressComponents";

        const double SampleSpacingMeters = 5000;
        const double SearchRadiusMeters = 3500;
        const int Parallel = 4;

        static readonly HttpClient Http = CreateClient();

        static HttpClient CreateClient()
        {
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
            return new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public static int EstimateRequests(IList<RoutePoint> route)
        {
            return 2 * SamplePoints(route).Count;
        }

        static List<RoutePoint> SamplePoints(IList<RoutePoint> route)
        {
            var result = new List<RoutePoint>();
            double next = 0;
            foreach (var p in route)
            {
                if (p.Distance >= next)
                {
                    result.Add(p);
                    next = p.Distance + SampleSpacingMeters;
                }
            }
            if (route.Count > 0 && result[result.Count - 1] != route[route.Count - 1])
            {
                result.Add(route[route.Count - 1]);
            }
            return result;
        }

        class Place
        {
            public string Locality;
            public double Lat, Lon;
            public string Category;   // bnb, hotel, hostel, camping, restaurant, fastfood
        }

        public static async Task<List<Town>> FindAsync(IList<RoutePoint> route, string apiKey,
            IProgress<string> progress, CancellationToken token)
        {
            var samples = SamplePoints(route);
            var places = new Dictionary<string, Place>();
            var gate = new SemaphoreSlim(Parallel);
            int done = 0;
            int total = samples.Count * 2;

            var tasks = new List<Task>();
            foreach (var sample in samples)
            {
                foreach (bool food in new[] { false, true })
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await gate.WaitAsync(token).ConfigureAwait(false);
                        try
                        {
                            var found = await SearchAsync(apiKey, sample, food, token).ConfigureAwait(false);
                            lock (places)
                            {
                                foreach (var kv in found) places[kv.Key] = kv.Value;
                            }
                        }
                        finally
                        {
                            gate.Release();
                            int n = Interlocked.Increment(ref done);
                            progress.Report(string.Format("Google Places: {0}/{1} zoekopdrachten…", n, total));
                        }
                    }, token));
                }
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            progress.Report("Resultaten groeperen per plaats…");
            return await Task.Run(() => BuildTowns(places.Values, route), token).ConfigureAwait(false);
        }

        static async Task<Dictionary<string, Place>> SearchAsync(string apiKey, RoutePoint center, bool food, CancellationToken token)
        {
            string types = food
                ? "\"restaurant\",\"fast_food_restaurant\""
                : "\"lodging\",\"bed_and_breakfast\",\"guest_house\",\"hotel\",\"hostel\",\"campground\"";
            string body = string.Format(CultureInfo.InvariantCulture,
                "{{\"includedTypes\":[{0}],\"maxResultCount\":20,\"locationRestriction\":{{\"circle\":{{\"center\":{{\"latitude\":{1:0.000000},\"longitude\":{2:0.000000}}},\"radius\":{3:0.0}}}}}}}",
                types, center.Lat, center.Lon, SearchRadiusMeters);

            using (var request = new HttpRequestMessage(HttpMethod.Post, Endpoint))
            {
                request.Headers.Add("X-Goog-Api-Key", apiKey);
                request.Headers.Add("X-Goog-FieldMask", FieldMask);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using (var response = await Http.SendAsync(request, token).ConfigureAwait(false))
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    SaveDebug(json);
                    Dictionary<string, object> root;
                    try
                    {
                        var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                        root = serializer.DeserializeObject(json) as Dictionary<string, object>;
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("Google Places: onleesbaar antwoord (HTTP " + (int)response.StatusCode + "), zie " + DebugFile);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException("Google Places: " + ErrorMessage(root, (int)response.StatusCode));
                    }

                    var result = new Dictionary<string, Place>();
                    object placesObj;
                    if (root == null || !root.TryGetValue("places", out placesObj)) return result;

                    foreach (var item in AsList(placesObj))
                    {
                        // Velden kunnen ontbreken (Google laat lege/nul-waarden weg): alles veilig uitlezen
                        var location = Get(item, "location") as Dictionary<string, object>;
                        string id = Get(item, "id") as string;
                        if (location == null || id == null) continue;
                        string category = Categorize(item, food);
                        if (category == null) continue;
                        result[id] = new Place
                        {
                            Lat = ToDouble(Get(location, "latitude")),
                            Lon = ToDouble(Get(location, "longitude")),
                            Locality = LocalityOf(item),
                            Category = category
                        };
                    }
                    return result;
                }
            }
        }

        static string ErrorMessage(Dictionary<string, object> root, int status)
        {
            object err;
            if (root != null && root.TryGetValue("error", out err))
            {
                var e = err as Dictionary<string, object>;
                string msg = e != null ? Get(e, "message") as string : null;
                if (msg != null)
                {
                    if (msg.Contains("has not been used") || msg.Contains("is disabled"))
                    {
                        msg += "\n\nZet 'Places API (New)' aan in je Google Cloud-project.";
                    }
                    else if (status == 400 && msg.ToLowerInvariant().Contains("api key"))
                    {
                        msg += "\n\nControleer je API-sleutel.";
                    }
                    return msg;
                }
            }
            return "HTTP " + status;
        }

        static string Categorize(Dictionary<string, object> item, bool food)
        {
            var types = new HashSet<string>();
            foreach (var s in AsStrings(Get(item, "types"))) types.Add(s);
            string primary = Get(item, "primaryType") as string ?? "";

            if (food)
            {
                if (primary == "fast_food_restaurant" || (types.Contains("fast_food_restaurant") && !types.Contains("restaurant")))
                    return "fastfood";
                return "restaurant";
            }

            if (primary == "hostel" || types.Contains("hostel")) return "hostel";
            if (primary == "campground" || primary == "camping_cabin" || primary == "rv_park") return "camping";
            if (primary == "bed_and_breakfast" || primary == "guest_house" || primary == "farmstay" ||
                primary == "cottage" || primary == "private_guest_room" || primary == "inn")
                return "bnb";
            if (types.Contains("bed_and_breakfast") || types.Contains("guest_house")) return "bnb";
            if (types.Contains("lodging") || types.Contains("hotel") || primary.Contains("hotel") || primary == "motel")
                return "hotel";
            return null;
        }

        /// <summary>Plaatsnaam uit de adresgegevens (locality, anders postal_town / gemeente).</summary>
        static string LocalityOf(Dictionary<string, object> item)
        {
            string fallback = null;
            foreach (var c in AsList(Get(item, "addressComponents")))
            {
                var types = AsStrings(Get(c, "types")).ToList();
                string name = (Get(c, "longText") ?? Get(c, "shortText")) as string;
                if (string.IsNullOrEmpty(name)) continue;
                if (types.Contains("locality")) return name;
                if (fallback == null && (types.Contains("postal_town") || types.Contains("administrative_area_level_3")))
                    fallback = name;
            }
            return fallback;
        }

        // ---------------------------------------------------------------- veilige JSON-helpers

        static object Get(Dictionary<string, object> dict, string key)
        {
            object value;
            return dict != null && dict.TryGetValue(key, out value) ? value : null;
        }

        static IEnumerable<Dictionary<string, object>> AsList(object value)
        {
            var list = value as IEnumerable;
            return list == null || value is string
                ? Enumerable.Empty<Dictionary<string, object>>()
                : list.OfType<Dictionary<string, object>>();
        }

        static IEnumerable<string> AsStrings(object value)
        {
            var list = value as IEnumerable;
            return list == null || value is string ? Enumerable.Empty<string>() : list.OfType<string>();
        }

        static double ToDouble(object value)
        {
            return value == null ? 0 : Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public static readonly string DebugFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AddWayPoints", "google-laatste-antwoord.json");

        static void SaveDebug(string json)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DebugFile));
                File.WriteAllText(DebugFile, json ?? "", Encoding.UTF8);
            }
            catch (Exception) { }
        }

        static List<Town> BuildTowns(IEnumerable<Place> places, IList<RoutePoint> route)
        {
            var towns = new List<Town>();
            foreach (var group in places.Where(p => !string.IsNullOrEmpty(p.Locality)).GroupBy(p => p.Locality))
            {
                // Plaats = middelpunt van de logies (of alles, als er geen logies zijn)
                var lodging = group.Where(p => p.Category != "restaurant" && p.Category != "fastfood").ToList();
                var basis = lodging.Count > 0 ? lodging : group.ToList();
                var town = new Town
                {
                    Id = StableId(group.Key),
                    Name = group.Key,
                    PlaceType = "google",
                    Lat = basis.Average(p => p.Lat),
                    Lon = basis.Average(p => p.Lon),
                    BedAndBreakfast = group.Count(p => p.Category == "bnb"),
                    Hotel = group.Count(p => p.Category == "hotel"),
                    Hostel = group.Count(p => p.Category == "hostel"),
                    Camping = group.Count(p => p.Category == "camping"),
                    Restaurant = group.Count(p => p.Category == "restaurant"),
                    FastFood = group.Count(p => p.Category == "fastfood")
                };
                towns.Add(town);
            }

            LodgingFinder.Assign(towns, Enumerable.Empty<KeyValuePair<string, double[]>>(), route);
            return towns;
        }

        static long StableId(string name)
        {
            using (var sha = SHA1.Create())
            {
                return BitConverter.ToInt64(sha.ComputeHash(Encoding.UTF8.GetBytes(name)), 0);
            }
        }
    }

    /// <summary>
    /// Bewaart de Google API-sleutel in het register: HKEY_CURRENT_USER\Software\AddWayPoints, waarde "GoogleApiKey".
    /// De app schrijft hem versleuteld weg (REG_BINARY, Windows DPAPI: enkel leesbaar voor jouw account).
    /// Een gewone tekstwaarde (REG_SZ) die je zelf aanmaakt, wordt ook aanvaard.
    /// </summary>
    internal static class ApiKeyStore
    {
        const string RegistryPath = @"Software\AddWayPoints";
        const string ValueName = "GoogleApiKey";

        static readonly string OldKeyFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AddWayPoints", "google-key.bin");

        public static string Load()
        {
            try
            {
                using (var reg = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    object value = reg != null ? reg.GetValue(ValueName) : null;
                    var bytes = value as byte[];
                    if (bytes != null)
                    {
                        return Encoding.UTF8.GetString(ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser));
                    }
                    var text = value as string;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }

                // Sleutel uit een vorige versie (bestand) overzetten naar het register
                if (File.Exists(OldKeyFile))
                {
                    string key = Encoding.UTF8.GetString(
                        ProtectedData.Unprotect(File.ReadAllBytes(OldKeyFile), null, DataProtectionScope.CurrentUser));
                    Save(key);
                    File.Delete(OldKeyFile);
                    return key;
                }
            }
            catch (Exception)
            {
                // onleesbaar = geen sleutel
            }
            return null;
        }

        public static void Save(string key)
        {
            using (var reg = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    reg.DeleteValue(ValueName, false);
                    return;
                }
                byte[] data = ProtectedData.Protect(Encoding.UTF8.GetBytes(key.Trim()), null, DataProtectionScope.CurrentUser);
                reg.SetValue(ValueName, data, RegistryValueKind.Binary);
            }
        }
    }
}
