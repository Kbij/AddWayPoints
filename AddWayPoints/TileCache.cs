using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddWayPoints
{
    /// <summary>Een bron van kaarttegels (slippy map, Web Mercator, 256 px).</summary>
    internal class TileSource
    {
        public string Name { get; set; }
        public string UrlFormat { get; set; }      // {s} {z} {x} {y}
        public string[] Subdomains { get; set; }
        public int MaxZoom { get; set; }
        public string Attribution { get; set; }
        public string CacheName { get; set; }

        public string Url(int z, int x, int y)
        {
            string s = Subdomains != null && Subdomains.Length > 0
                ? Subdomains[Math.Abs(x + y) % Subdomains.Length]
                : "";
            return UrlFormat.Replace("{s}", s).Replace("{z}", z.ToString())
                            .Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
        }

        public static readonly TileSource CyclOsm = new TileSource
        {
            Name = "Fietskaart (CyclOSM)",
            UrlFormat = "https://{s}.tile-cyclosm.openstreetmap.fr/cyclosm/{z}/{x}/{y}.png",
            Subdomains = new[] { "a", "b", "c" },
            MaxZoom = 18,
            Attribution = "© OpenStreetMap-bijdragers, CyclOSM",
            CacheName = "cyclosm"
        };

        public static readonly TileSource Osm = new TileSource
        {
            Name = "OpenStreetMap",
            UrlFormat = "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            MaxZoom = 19,
            Attribution = "© OpenStreetMap-bijdragers",
            CacheName = "osm"
        };
    }

    /// <summary>
    /// Laadt kaarttegels op de achtergrond: eerst uit de schijfcache
    /// (%LOCALAPPDATA%\AddWayPoints\tiles), anders van het internet.
    /// Alle toegang tot het geheugen gebeurt op de UI-thread.
    /// </summary>
    internal class TileCache : IDisposable
    {
        const int MaxMemoryTiles = 400;

        static readonly HttpClient Http = CreateClient();

        readonly Control mOwner;
        readonly Dictionary<string, Bitmap> mMemory = new Dictionary<string, Bitmap>();
        readonly LinkedList<string> mLru = new LinkedList<string>();
        readonly HashSet<string> mPending = new HashSet<string>();
        readonly HashSet<string> mFailed = new HashSet<string>();
        readonly SemaphoreSlim mThrottle = new SemaphoreSlim(4);
        readonly object mWantedLock = new object();
        HashSet<string> mWanted = new HashSet<string>();
        readonly string mCacheRoot;

        public TileCache(Control owner)
        {
            mOwner = owner;
            mCacheRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AddWayPoints", "tiles");
        }

        static HttpClient CreateClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            // De OSM-tegelservers vereisen een herkenbare User-Agent.
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AddWayPoints/1.0 (GPX km-marker tool)");
            return client;
        }

        static string Key(TileSource source, int z, int x, int y)
        {
            return source.CacheName + "/" + z + "/" + x + "/" + y;
        }

        /// <summary>Welke tegels momenteel zichtbaar zijn; tegels die niet meer nodig zijn worden niet gedownload.</summary>
        public void SetWanted(HashSet<string> keys)
        {
            lock (mWantedLock) { mWanted = keys; }
        }

        public static string WantedKey(TileSource source, int z, int x, int y)
        {
            return Key(source, z, x, y);
        }

        /// <summary>Geeft de tegel als die al in het geheugen zit, zonder te laden.</summary>
        public Bitmap Peek(TileSource source, int z, int x, int y)
        {
            Bitmap bmp;
            return mMemory.TryGetValue(Key(source, z, x, y), out bmp) ? bmp : null;
        }

        /// <summary>Geeft de tegel of null; in dat geval wordt hij op de achtergrond geladen.</summary>
        public Bitmap Get(TileSource source, int z, int x, int y)
        {
            string key = Key(source, z, x, y);
            Bitmap bmp;
            if (mMemory.TryGetValue(key, out bmp))
            {
                mLru.Remove(key);
                mLru.AddFirst(key);
                return bmp;
            }
            if (!mPending.Contains(key) && !mFailed.Contains(key))
            {
                mPending.Add(key);
                string url = source.Url(z, x, y);
                string file = Path.Combine(mCacheRoot, source.CacheName, z.ToString(), x.ToString(), y + ".png");
                Task.Run(() => LoadAsync(key, url, file));
            }
            return null;
        }

        async Task LoadAsync(string key, string url, string file)
        {
            byte[] data = null;
            bool failed = false;
            try
            {
                if (File.Exists(file))
                {
                    data = File.ReadAllBytes(file);
                }
                else
                {
                    bool wanted;
                    lock (mWantedLock) { wanted = mWanted.Contains(key); }
                    if (wanted)
                    {
                        await mThrottle.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            data = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
                        }
                        finally
                        {
                            mThrottle.Release();
                        }
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(file));
                            File.WriteAllBytes(file, data);
                        }
                        catch (IOException) { /* cache is best effort */ }
                    }
                }
            }
            catch (Exception)
            {
                failed = true;
            }

            Bitmap bitmap = null;
            if (data != null)
            {
                try
                {
                    using (var ms = new MemoryStream(data))
                    using (var img = Image.FromStream(ms))
                    {
                        bitmap = new Bitmap(img);
                    }
                }
                catch (Exception)
                {
                    failed = true;
                }
            }

            Deliver(key, bitmap, failed);
        }

        void Deliver(string key, Bitmap bitmap, bool failed)
        {
            if (mOwner.IsDisposed || !mOwner.IsHandleCreated)
            {
                if (bitmap != null) bitmap.Dispose();
                return;
            }
            try
            {
                mOwner.BeginInvoke((Action)(() =>
                {
                    mPending.Remove(key);
                    if (failed)
                    {
                        mFailed.Add(key);
                    }
                    if (bitmap != null)
                    {
                        Add(key, bitmap);
                        mOwner.Invalidate();
                    }
                }));
            }
            catch (InvalidOperationException)
            {
                if (bitmap != null) bitmap.Dispose();
            }
        }

        void Add(string key, Bitmap bitmap)
        {
            Bitmap old;
            if (mMemory.TryGetValue(key, out old))
            {
                old.Dispose();
                mLru.Remove(key);
            }
            mMemory[key] = bitmap;
            mLru.AddFirst(key);

            while (mLru.Count > MaxMemoryTiles)
            {
                string last = mLru.Last.Value;
                mLru.RemoveLast();
                mMemory[last].Dispose();
                mMemory.Remove(last);
            }
        }

        /// <summary>Opnieuw proberen na bv. een weggevallen internetverbinding.</summary>
        public void ClearFailures()
        {
            mFailed.Clear();
        }

        public void Dispose()
        {
            foreach (var bmp in mMemory.Values)
            {
                bmp.Dispose();
            }
            mMemory.Clear();
            mLru.Clear();
        }
    }
}
