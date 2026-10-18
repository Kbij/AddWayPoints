using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AddWayPoints
{
    /// <summary>
    /// Live kaart (OpenStreetMap / CyclOSM) met de route en de markers.
    /// Muiswiel = zoomen, slepen = verschuiven, dubbelklik = inzoomen.
    /// Hover over de route toont de kilometerstand.
    /// </summary>
    internal class RoutePreview : Control
    {
        static readonly Color RouteColor = Color.FromArgb(37, 99, 235);
        static readonly Color MarkerColor = Color.FromArgb(234, 88, 12);
        static readonly Color DayColor = Color.FromArgb(185, 28, 28);
        static readonly Color PlaceColor = Color.FromArgb(126, 34, 206);
        static readonly Color PanelColor = Color.FromArgb(235, 15, 23, 42);

        const int TileSize = 256;
        const int MinZoom = 2;

        static readonly TileSource[] Layers = { TileSource.CyclOsm, TileSource.Osm, null };

        IList<RoutePoint> mPoints = new List<RoutePoint>();
        IList<KmMarker> mMarkers = new List<KmMarker>();
        IList<PlaceMark> mPlaces = new List<PlaceMark>();
        IList<Town> mTowns = new List<Town>();

        // Route in genormaliseerde Web Mercator-coördinaten (0..1)
        PointD[] mWorld = new PointD[0];
        PointF[] mScreen = new PointF[0];

        int mZoom = 5;
        double mCenterX = 0.5, mCenterY = 0.5;
        int mLayerIndex = 0;
        readonly TileCache mTiles;

        bool mDragging;
        Point mDragStart, mLastMouse;
        int mHoverIndex = -1;
        KmMarker mSelected;

        Rectangle mBtnZoomIn, mBtnZoomOut, mBtnFit, mBtnLayer;

        struct PointD
        {
            public double X, Y;
            public PointD(double x, double y) { X = x; Y = y; }
        }

        public RoutePreview()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(241, 245, 249);
            mTiles = new TileCache(this);
            ResetView();
        }

        /// <summary>Standaardbeeld zonder route: België en omstreken.</summary>
        void ResetView()
        {
            PointD w = ToWorld(50.6, 4.6);
            mCenterX = w.X;
            mCenterY = w.Y;
            mZoom = 7;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mTiles.Dispose();
            }
            base.Dispose(disposing);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double KmOffset { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KmMarker SelectedMarker
        {
            get { return mSelected; }
            set
            {
                mSelected = value;
                if (value != null)
                {
                    // Geselecteerde marker in beeld brengen
                    PointF p = Project(value.Lat, value.Lon);
                    if (!ClientRectangle.Contains(Point.Round(p)) || p.X < 40 || p.Y < 40 || p.X > Width - 40 || p.Y > Height - 40)
                    {
                        PointD w = ToWorld(value.Lat, value.Lon);
                        mCenterX = w.X;
                        mCenterY = w.Y;
                    }
                }
                Invalidate();
            }
        }

        TileSource Layer
        {
            get { return Layers[mLayerIndex]; }
        }

        public void SetRoute(IList<RoutePoint> points)
        {
            mPoints = points ?? new List<RoutePoint>();
            mWorld = new PointD[mPoints.Count];
            for (int i = 0; i < mPoints.Count; i++)
            {
                mWorld[i] = ToWorld(mPoints[i].Lat, mPoints[i].Lon);
            }
            mHoverIndex = -1;
            ZoomToFit();
        }

        public void SetMarkers(IList<KmMarker> markers)
        {
            mMarkers = markers ?? new List<KmMarker>();
            if (mSelected != null && !mMarkers.Contains(mSelected))
            {
                mSelected = null;
            }
            Invalidate();
        }

        /// <summary>Plaatsen met voldoende logies (kandidaten voor een overnachting).</summary>
        public void SetTowns(IList<Town> towns)
        {
            mTowns = towns ?? new List<Town>();
            Invalidate();
        }

        public void SetPlaces(IList<PlaceMark> places)
        {
            mPlaces = places ?? new List<PlaceMark>();
            Invalidate();
        }

        public void ZoomToFit()
        {
            if (mWorld.Length == 0)
            {
                ResetView();
                Invalidate();
                return;
            }
            if (Width < 50 || Height < 50)
            {
                Invalidate();
                return;
            }

            double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
            foreach (var w in mWorld)
            {
                minX = Math.Min(minX, w.X); maxX = Math.Max(maxX, w.X);
                minY = Math.Min(minY, w.Y); maxY = Math.Max(maxY, w.Y);
            }
            mCenterX = (minX + maxX) / 2;
            mCenterY = (minY + maxY) / 2;

            double spanX = Math.Max(maxX - minX, 1e-9) * TileSize;
            double spanY = Math.Max(maxY - minY, 1e-9) * TileSize;
            double scale = Math.Min((Width - 80) / spanX, (Height - 80) / spanY);
            mZoom = Clamp((int)Math.Floor(Math.Log(scale, 2)), MinZoom, MaxZoom);
            Invalidate();
        }

        int MaxZoom
        {
            get { return Layer != null ? Layer.MaxZoom : 18; }
        }

        static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        // ------------------------------------------------------------------ Projectie

        static PointD ToWorld(double lat, double lon)
        {
            lat = Math.Max(-85.05112878, Math.Min(85.05112878, lat));
            double x = (lon + 180.0) / 360.0;
            double sin = Math.Sin(lat * Math.PI / 180.0);
            double y = 0.5 - Math.Log((1 + sin) / (1 - sin)) / (4 * Math.PI);
            return new PointD(x, y);
        }

        double WorldSize
        {
            get { return TileSize * Math.Pow(2, mZoom); }
        }

        PointF WorldToScreen(PointD w)
        {
            double size = WorldSize;
            return new PointF(
                (float)((w.X - mCenterX) * size + Width / 2.0),
                (float)((w.Y - mCenterY) * size + Height / 2.0));
        }

        PointD ScreenToWorld(Point p)
        {
            double size = WorldSize;
            return new PointD(mCenterX + (p.X - Width / 2.0) / size, mCenterY + (p.Y - Height / 2.0) / size);
        }

        PointF Project(double lat, double lon)
        {
            return WorldToScreen(ToWorld(lat, lon));
        }

        // ------------------------------------------------------------------ Tekenen

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            DrawTiles(g);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (mWorld.Length < 2)
            {
                // Nog geen route: kaart en knoppen wel tonen, met een hint onderaan
                mScreen = new PointF[0];
                using (var font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold))
                {
                    const string hint = "Open een GPX-bestand om de route op de kaart te zien";
                    SizeF size = g.MeasureString(hint, font);
                    var box = new RectangleF((Width - size.Width) / 2 - 12, Height - size.Height - 48, size.Width + 24, size.Height + 14);
                    using (var path = RoundedRect(box, 6))
                    using (var bg = new SolidBrush(PanelColor))
                    {
                        g.FillPath(bg, path);
                    }
                    g.DrawString(hint, font, Brushes.White, box.X + 12, box.Y + 7);
                }
                DrawControls(g);
                return;
            }

            mScreen = new PointF[mWorld.Length];
            for (int i = 0; i < mWorld.Length; i++)
            {
                mScreen[i] = WorldToScreen(mWorld[i]);
            }

            using (var halo = new Pen(Color.FromArgb(220, Color.White), 7) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pen = new Pen(RouteColor, 4) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLines(halo, mScreen);
                g.DrawLines(pen, mScreen);
            }

            using (var labelFont = new Font(Font.FontFamily, 8f, FontStyle.Bold))
            using (var boldFont = new Font(Font.FontFamily, 8.5f, FontStyle.Bold))
            {
                DrawTowns(g);
                DrawIntervalMarkers(g, labelFont);
                DrawDayMarkers(g, boldFont);
                DrawPlaces(g, labelFont);
                DrawEndpoint(g, mScreen[0], Color.FromArgb(22, 163, 74), "Start", boldFont);
                DrawEndpoint(g, mScreen[mScreen.Length - 1], Color.FromArgb(15, 23, 42), "Finish", boldFont);
                DrawSelection(g);
                DrawHover(g, boldFont);
            }

            DrawControls(g);
        }

        void DrawTiles(Graphics g)
        {
            TileSource layer = Layer;
            if (layer == null)
            {
                mTiles.SetWanted(new HashSet<string>());
                return;
            }

            int n = 1 << mZoom;
            double size = WorldSize;
            double left = mCenterX * size - Width / 2.0;
            double top = mCenterY * size - Height / 2.0;
            int tx0 = (int)Math.Floor(left / TileSize), tx1 = (int)Math.Floor((left + Width) / TileSize);
            int ty0 = Math.Max(0, (int)Math.Floor(top / TileSize)), ty1 = Math.Min(n - 1, (int)Math.Floor((top + Height) / TileSize));

            var wanted = new HashSet<string>();
            for (int ty = ty0; ty <= ty1; ty++)
            {
                for (int tx = tx0; tx <= tx1; tx++)
                {
                    wanted.Add(TileCache.WantedKey(layer, mZoom, ((tx % n) + n) % n, ty));
                }
            }
            mTiles.SetWanted(wanted);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            for (int ty = ty0; ty <= ty1; ty++)
            {
                for (int tx = tx0; tx <= tx1; tx++)
                {
                    int wx = ((tx % n) + n) % n;
                    var dest = new Rectangle((int)Math.Round(tx * TileSize - left), (int)Math.Round(ty * TileSize - top), TileSize, TileSize);
                    Bitmap tile = mTiles.Get(layer, mZoom, wx, ty);
                    if (tile != null)
                    {
                        g.DrawImage(tile, dest);
                    }
                    else
                    {
                        DrawParentFallback(g, layer, wx, ty, dest);
                    }
                }
            }
            g.PixelOffsetMode = PixelOffsetMode.Default;

            // Kaart iets lichter maken zodat route en markers eruit springen
            using (var veil = new SolidBrush(Color.FromArgb(60, Color.White)))
            {
                g.FillRectangle(veil, ClientRectangle);
            }
        }

        /// <summary>Zolang een tegel laadt: toon een uitvergrote versie van een lager zoomniveau.</summary>
        void DrawParentFallback(Graphics g, TileSource layer, int x, int y, Rectangle dest)
        {
            for (int up = 1; up <= 5 && mZoom - up >= 0; up++)
            {
                Bitmap parent = mTiles.Peek(layer, mZoom - up, x >> up, y >> up);
                if (parent != null)
                {
                    int part = TileSize >> up;
                    var src = new Rectangle((x & ((1 << up) - 1)) * part, (y & ((1 << up) - 1)) * part, part, part);
                    g.InterpolationMode = InterpolationMode.Bilinear;
                    g.DrawImage(parent, dest, src, GraphicsUnit.Pixel);
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    return;
                }
            }
            using (var brush = new SolidBrush(Color.FromArgb(226, 232, 240)))
            {
                g.FillRectangle(brush, dest);
            }
        }

        void DrawIntervalMarkers(Graphics g, Font font)
        {
            PointF lastLabel = new PointF(float.MinValue, float.MinValue);
            using (var fill = new SolidBrush(Color.White))
            using (var pen = new Pen(MarkerColor, 2.5f))
            using (var text = new SolidBrush(Color.FromArgb(154, 52, 18)))
            {
                foreach (var m in mMarkers)
                {
                    if (m.Kind != MarkerKind.Interval) continue;
                    PointF p = Project(m.Lat, m.Lon);
                    if (!IsVisible(p)) continue;
                    var r = new RectangleF(p.X - 5, p.Y - 5, 10, 10);
                    g.FillEllipse(fill, r);
                    g.DrawEllipse(pen, r);

                    // Labels enkel als er ruimte is, anders wordt het onleesbaar
                    if (Distance(p, lastLabel) > 40)
                    {
                        DrawLabel(g, m.Name, font, text, new PointF(p.X + 7, p.Y - 18));
                        lastLabel = p;
                    }
                }
            }
        }

        void DrawTowns(Graphics g)
        {
            using (var fill = new SolidBrush(Color.FromArgb(120, 71, 85, 105)))
            {
                foreach (var town in mTowns)
                {
                    PointF p = Project(town.Lat, town.Lon);
                    if (!IsVisible(p)) continue;
                    g.FillEllipse(fill, p.X - 3, p.Y - 3, 6, 6);
                }
            }
        }

        void DrawDayMarkers(Graphics g, Font font)
        {
            using (var fill = new SolidBrush(DayColor))
            using (var white = new SolidBrush(Color.White))
            using (var outline = new Pen(Color.White, 2))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                foreach (var m in mMarkers)
                {
                    if (m.Kind != MarkerKind.DayEnd) continue;
                    PointF p = Project(m.Lat, m.Lon);
                    if (!IsVisible(p)) continue;
                    var r = new RectangleF(p.X - 11, p.Y - 11, 22, 22);
                    g.FillEllipse(fill, r);
                    g.DrawEllipse(outline, r);
                    g.DrawString(m.Day.ToString(), font, white, r, format);

                    // Naam van de overnachtingsplaats ("Dag 3: Lannion" -> "Lannion")
                    int colon = m.Name.IndexOf(':');
                    if (colon > 0)
                    {
                        DrawLabel(g, m.Name.Substring(colon + 1).Trim(), font, fill, new PointF(p.X + 13, p.Y - 8));
                    }
                }
            }
        }

        void DrawPlaces(Graphics g, Font font)
        {
            using (var fill = new SolidBrush(PlaceColor))
            using (var text = new SolidBrush(PlaceColor))
            {
                foreach (var place in mPlaces)
                {
                    PointF p = Project(place.Lat, place.Lon);
                    if (!IsVisible(p)) continue;
                    var diamond = new[]
                    {
                        new PointF(p.X, p.Y - 8), new PointF(p.X + 8, p.Y),
                        new PointF(p.X, p.Y + 8), new PointF(p.X - 8, p.Y)
                    };
                    g.FillPolygon(fill, diamond);
                    g.DrawPolygon(Pens.White, diamond);
                    DrawLabel(g, place.Name, font, text, new PointF(p.X + 10, p.Y - 8));
                }
            }
        }

        void DrawEndpoint(Graphics g, PointF p, Color color, string label, Font font)
        {
            using (var fill = new SolidBrush(color))
            using (var outline = new Pen(Color.White, 2.5f))
            {
                var r = new RectangleF(p.X - 8, p.Y - 8, 16, 16);
                g.FillEllipse(fill, r);
                g.DrawEllipse(outline, r);
                DrawLabel(g, label, font, fill, new PointF(p.X + 11, p.Y + 2));
            }
        }

        void DrawSelection(Graphics g)
        {
            if (mSelected == null) return;
            PointF p = Project(mSelected.Lat, mSelected.Lon);
            using (var pen = new Pen(Color.FromArgb(200, 15, 23, 42), 2.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawEllipse(pen, p.X - 17, p.Y - 17, 34, 34);
            }
        }

        void DrawHover(Graphics g, Font font)
        {
            if (mDragging || mHoverIndex < 0 || mHoverIndex >= mScreen.Length) return;
            PointF p = mScreen[mHoverIndex];
            double km = KmOffset + mPoints[mHoverIndex].Distance / 1000.0;

            using (var fill = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillEllipse(fill, p.X - 6, p.Y - 6, 12, 12);
                g.FillEllipse(Brushes.White, p.X - 2.5f, p.Y - 2.5f, 5, 5);
                string text = string.Format("km {0:N1}", km);
                SizeF size = g.MeasureString(text, font);
                var box = new RectangleF(p.X + 12, p.Y - size.Height - 10, size.Width + 12, size.Height + 8);
                if (box.Right > Width) box.X = p.X - box.Width - 12;
                if (box.Top < 0) box.Y = p.Y + 10;
                using (var path = RoundedRect(box, 4))
                {
                    g.FillPath(fill, path);
                }
                g.DrawString(text, font, Brushes.White, box.X + 6, box.Y + 4);
            }
        }

        void DrawControls(Graphics g)
        {
            // Zoomknoppen rechtsboven
            const int b = 30;
            int x = Width - b - 10;
            mBtnZoomIn = new Rectangle(x, 10, b, b);
            mBtnZoomOut = new Rectangle(x, 10 + b, b, b);
            mBtnFit = new Rectangle(x, 20 + 2 * b, b, b);

            using (var bg = new SolidBrush(Color.FromArgb(245, Color.White)))
            using (var border = new Pen(Color.FromArgb(203, 213, 225)))
            using (var ink = new Pen(Color.FromArgb(30, 41, 59), 2))
            {
                foreach (var r in new[] { mBtnZoomIn, mBtnZoomOut, mBtnFit })
                {
                    g.FillRectangle(bg, r);
                    g.DrawRectangle(border, r);
                }
                int cx = x + b / 2;
                g.DrawLine(ink, cx - 7, 10 + b / 2, cx + 7, 10 + b / 2);
                g.DrawLine(ink, cx, 10 + b / 2 - 7, cx, 10 + b / 2 + 7);
                g.DrawLine(ink, cx - 7, 10 + b + b / 2, cx + 7, 10 + b + b / 2);
                var f = Rectangle.Inflate(mBtnFit, -8, -8);
                g.DrawRectangle(ink, f);
            }

            // Kaartlaag-knop linksboven
            using (var font = new Font(Font.FontFamily, 8.5f, FontStyle.Bold))
            {
                string text = "Kaart: " + (Layer != null ? Layer.Name : "geen") + "  ⟳";
                SizeF size = g.MeasureString(text, font);
                mBtnLayer = new Rectangle(10, 10, (int)size.Width + 16, (int)size.Height + 10);
                using (var path = RoundedRect(mBtnLayer, 5))
                using (var bg = new SolidBrush(PanelColor))
                {
                    g.FillPath(bg, path);
                }
                g.DrawString(text, font, Brushes.White, mBtnLayer.X + 8, mBtnLayer.Y + 5);
            }

            // Bronvermelding (verplicht voor OSM-tegels) en zoomniveau
            using (var font = new Font(Font.FontFamily, 7.5f))
            using (var bg = new SolidBrush(Color.FromArgb(200, Color.White)))
            using (var ink = new SolidBrush(Color.FromArgb(51, 65, 85)))
            {
                string text = (Layer != null ? Layer.Attribution + "  ·  " : "") + "zoom " + mZoom;
                SizeF size = g.MeasureString(text, font);
                var r = new RectangleF(Width - size.Width - 6, Height - size.Height - 2, size.Width + 6, size.Height + 2);
                g.FillRectangle(bg, r);
                g.DrawString(text, font, ink, r.X + 3, r.Y + 1);
            }
        }

        static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        void DrawLabel(Graphics g, string text, Font font, Brush brush, PointF at)
        {
            SizeF size = g.MeasureString(text, font);
            using (var bg = new SolidBrush(Color.FromArgb(215, Color.White)))
            {
                g.FillRectangle(bg, at.X - 1, at.Y, size.Width, size.Height);
            }
            g.DrawString(text, font, brush, at);
        }

        bool IsVisible(PointF p)
        {
            return p.X > -50 && p.Y > -50 && p.X < Width + 50 && p.Y < Height + 50;
        }

        static float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // ------------------------------------------------------------------ Interactie

        void ZoomAt(Point anchor, int delta)
        {
            int newZoom = Clamp(mZoom + delta, MinZoom, MaxZoom);
            if (newZoom == mZoom) return;

            // Het punt onder de muis blijft op dezelfde plaats staan
            PointD before = ScreenToWorld(anchor);
            mZoom = newZoom;
            PointD after = ScreenToWorld(anchor);
            mCenterX += before.X - after.X;
            mCenterY += before.Y - after.Y;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ZoomAt(e.Location, e.Delta > 0 ? 1 : -1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                mDragging = true;
                mDragStart = e.Location;
                mLastMouse = e.Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            bool click = Math.Abs(e.X - mDragStart.X) < 4 && Math.Abs(e.Y - mDragStart.Y) < 4;
            mDragging = false;
            Cursor = Cursors.Default;

            if (click)
            {
                var center = new Point(Width / 2, Height / 2);
                if (mBtnZoomIn.Contains(e.Location)) ZoomAt(center, 1);
                else if (mBtnZoomOut.Contains(e.Location)) ZoomAt(center, -1);
                else if (mBtnFit.Contains(e.Location)) ZoomToFit();
                else if (mBtnLayer.Contains(e.Location))
                {
                    mLayerIndex = (mLayerIndex + 1) % Layers.Length;
                    mZoom = Clamp(mZoom, MinZoom, MaxZoom);
                    mTiles.ClearFailures();
                    Invalidate();
                }
            }
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (!IsOnButton(e.Location))
            {
                ZoomAt(e.Location, e.Button == MouseButtons.Right ? -1 : 1);
            }
        }

        bool IsOnButton(Point p)
        {
            return mBtnZoomIn.Contains(p) || mBtnZoomOut.Contains(p) || mBtnFit.Contains(p) || mBtnLayer.Contains(p);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mDragging)
            {
                double size = WorldSize;
                mCenterX -= (e.X - mLastMouse.X) / size;
                mCenterY -= (e.Y - mLastMouse.Y) / size;
                mCenterY = Math.Max(0, Math.Min(1, mCenterY));
                mLastMouse = e.Location;
                Cursor = Cursors.SizeAll;
                Invalidate();
                return;
            }

            Cursor = IsOnButton(e.Location) ? Cursors.Hand : Cursors.Default;

            int best = -1;
            float bestDistance = 15;   // pixels
            for (int i = 0; i < mScreen.Length; i++)
            {
                float d = Distance(mScreen[i], e.Location);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            if (best != mHoverIndex)
            {
                mHoverIndex = best;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (mHoverIndex != -1)
            {
                mHoverIndex = -1;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var center = new Point(Width / 2, Height / 2);
            double step = 100 / WorldSize;
            switch (e.KeyCode)
            {
                case Keys.Add:
                case Keys.Oemplus: ZoomAt(center, 1); break;
                case Keys.Subtract:
                case Keys.OemMinus: ZoomAt(center, -1); break;
                case Keys.Home: ZoomToFit(); break;
                case Keys.Left: mCenterX -= step; Invalidate(); break;
                case Keys.Right: mCenterX += step; Invalidate(); break;
                case Keys.Up: mCenterY -= step; Invalidate(); break;
                case Keys.Down: mCenterY += step; Invalidate(); break;
            }
        }
    }
}
