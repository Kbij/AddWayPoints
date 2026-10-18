using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace AddWayPoints
{
    public partial class MainForm : Form
    {
        /// <summary>Segmenten waarvan begin en einde dichter dan dit bij elkaar liggen, sluiten op elkaar aan.</summary>
        const double ALLOWED_TRACK_DISTANCE = 10;

        GpxRoute mRoute;
        List<KmMarker> mMarkers = new List<KmMarker>();
        List<PlaceMark> mPlaces = new List<PlaceMark>();
        string mLastOutput;

        // Overnachtingsplanning
        List<Town> mTowns;              // logies langs de route (opgehaald via OpenStreetMap)
        List<Stage> mStages;            // gekozen etappes, null = geen planning
        bool mPlanning;

        public MainForm()
        {
            InitializeComponent();
            try
            {
                // Zelfde icoon als de .exe (ApplicationIcon in het project)
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
                // geen icoon is geen ramp
            }
            cbxSymbol.SelectedIndex = 0;
            cbxSource.SelectedIndex = 0;
            UpdateUiState();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Bij Windows-schaal van 125/150% kan het venster hoger worden dan het scherm;
            // dan vallen de onderste knoppen weg. In dat geval gewoon maximaliseren.
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            if (Width > workingArea.Width || Height > workingArea.Height)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        // ------------------------------------------------------------------ Route openen

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            dlgOpenFile.Filter = "GPX-bestanden (*.gpx)|*.gpx|Alle bestanden (*.*)|*.*";
            dlgOpenFile.FileName = "";
            if (dlgOpenFile.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                AddLog("Bestand geopend: " + dlgOpenFile.FileName);
                var route = GpxRoute.Load(dlgOpenFile.FileName);

                foreach (var track in route.Tracks)
                {
                    AddLog(string.Format("  Track '{0}': {1} segment(en), {2:N1} km",
                        track.Name ?? "(naamloos)", track.Segments.Count, track.Length / 1000));
                }

                if (chkSort.Checked)
                {
                    var ordered = TrackSorter.Sort(route.Tracks, ALLOWED_TRACK_DISTANCE, AddLog);
                    string sortedFile = Path.Combine(Path.GetDirectoryName(route.FileName),
                        Path.GetFileNameWithoutExtension(route.FileName) + "-sorted.gpx");
                    route = GpxRoute.SaveSorted(route, ordered, sortedFile);
                    AddLog("Gesorteerd bestand aangemaakt: " + sortedFile + " (verder wordt hiermee gewerkt)");
                }

                foreach (var gap in route.Gaps)
                {
                    AddLog(string.Format("  Let op: sprong van {0:N0} m bij km {1:N1} (rechte lijn meegeteld)",
                        gap.Value, gap.Key / 1000));
                }

                if (route.Points.Count < 2)
                {
                    AddLog("Geen trackpunten gevonden in dit bestand.");
                    return;
                }

                mRoute = route;
                mPlaces.Clear();
                mTowns = null;
                mStages = null;
                lvStages.Items.Clear();
                routePreview.SetTowns(null);
                mLastOutput = null;
                lnkOutput.Visible = false;

                AddLog(string.Format("Totale afstand: {0:N1} km, {1:N0} punten", route.TotalDistance / 1000, route.Points.Count));
                AddLog("");

                lblFile.Text = Path.GetFileName(route.FileName);
                routePreview.SetRoute(route.Points);
                routePreview.SetPlaces(mPlaces);
                RegenerateMarkers();
            }
            catch (Exception ex)
            {
                AddLog("Fout bij het lezen: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Fout bij het lezen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                UpdateUiState();
            }
        }

        // ------------------------------------------------------------------ Markers

        private void Settings_Changed(object sender, EventArgs e)
        {
            RegenerateMarkers();
        }

        MarkerSettings CurrentSettings()
        {
            return new MarkerSettings
            {
                IntervalKm = (double)numInterval.Value,
                OffsetKm = (double)numOffset.Value,
                DayKm = (double)numDay.Value,
                NameFormat = txtName.Text
            };
        }

        /// <summary>Berekent de markers opnieuw en toont ze meteen in kaart en lijst (nog niets wegschrijven).</summary>
        void RegenerateMarkers()
        {
            if (mRoute == null)
            {
                return;
            }

            var settings = CurrentSettings();
            if (mStages != null)
            {
                settings.DayKm = 0;   // dag-markers komen uit de planning, niet op vaste afstand
            }
            mMarkers = MarkerGenerator.Generate(mRoute.Points, settings);
            if (mStages != null)
            {
                mMarkers.AddRange(StageMarkers(settings.OffsetKm));
                mMarkers.Sort((a, b) => a.RouteDistance.CompareTo(b.RouteDistance));
            }

            routePreview.KmOffset = settings.OffsetKm;
            routePreview.SetMarkers(mMarkers);

            lvMarkers.BeginUpdate();
            lvMarkers.Items.Clear();
            foreach (var marker in mMarkers)
            {
                var item = new ListViewItem(marker.Name);
                item.SubItems.Add(marker.LabelKm.ToString("N1"));
                item.SubItems.Add((marker.RouteDistance / 1000).ToString("N1"));
                item.SubItems.Add(GeoMath.FormatCoordinate(marker.Lat));
                item.SubItems.Add(GeoMath.FormatCoordinate(marker.Lon));
                item.Tag = marker;
                if (marker.Kind == MarkerKind.DayEnd)
                {
                    item.ForeColor = System.Drawing.Color.FromArgb(185, 28, 28);
                    item.Font = new System.Drawing.Font(lvMarkers.Font, System.Drawing.FontStyle.Bold);
                }
                lvMarkers.Items.Add(item);
            }
            lvMarkers.EndUpdate();

            UpdateStatus();
        }

        private void LvMarkers_SelectedIndexChanged(object sender, EventArgs e)
        {
            routePreview.SelectedMarker = lvMarkers.SelectedItems.Count > 0
                ? lvMarkers.SelectedItems[0].Tag as KmMarker
                : null;
        }

        private void OutputMode_Changed(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            if (mRoute == null)
            {
                return;
            }
            if (mMarkers.Count == 0)
            {
                MessageBox.Show(this, "Er zijn geen markers om te schrijven: de route is korter dan het interval.",
                    "Geen markers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string symbol = cbxSymbol.Text;
                if (rbSeparate.Checked)
                {
                    string file = GpxMarkerWriter.WriteSeparateFile(mRoute, mMarkers, symbol);
                    AddLog(string.Format("{0} markers geschreven naar {1}", mMarkers.Count, file));
                    SetOutput(file);
                }
                else
                {
                    int removed;
                    string backup = GpxMarkerWriter.WriteIntoRoute(mRoute, mMarkers, symbol, chkRemoveOther.Checked, out removed);
                    AddLog("Backup: " + backup);
                    if (removed > 0)
                    {
                        AddLog(string.Format("{0} bestaande waypoint(s) verwijderd", removed));
                    }
                    AddLog(string.Format("{0} markers toegevoegd aan {1}", mMarkers.Count, mRoute.FileName));
                    SetOutput(mRoute.FileName);
                }
                lblStatus.Text = string.Format("{0} markers geschreven naar {1}", mMarkers.Count, Path.GetFileName(mLastOutput));
            }
            catch (Exception ex)
            {
                AddLog("Fout bij het schrijven: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Fout bij het schrijven", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SetOutput(string file)
        {
            mLastOutput = file;
            lnkOutput.Text = "Toon in Verkenner: " + Path.GetFileName(file);
            lnkOutput.Visible = true;
        }

        private void LnkOutput_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(mLastOutput) && File.Exists(mLastOutput))
            {
                Process.Start("explorer.exe", "/select,\"" + mLastOutput + "\"");
            }
        }

        // ------------------------------------------------------------------ Overnachtingen

        PlanSettings CurrentPlanSettings()
        {
            return new PlanSettings
            {
                DayKm = (double)numDay.Value,
                MaxOffRouteKm = (double)numMaxOff.Value,
                MinLodgings = (int)numMinLodging.Value,
                MinRestaurants = (int)numMinFood.Value,
                IncludeCamping = chkCamping.Checked
            };
        }

        private void PlanSettings_Changed(object sender, EventArgs e)
        {
            // Logies al opgehaald? Dan meteen opnieuw plannen (gaat snel, geen internet nodig).
            if (mTowns != null && numDay.Value > 0)
            {
                RunPlan();
            }
            else if (mStages != null)
            {
                mStages = null;
                lvStages.Items.Clear();
            }
            RegenerateMarkers();
        }

        private async void BtnPlan_Click(object sender, EventArgs e)
        {
            if (mRoute == null || mPlanning)
            {
                return;
            }
            if (numDay.Value <= 0)
            {
                MessageBox.Show(this, "Vul eerst in hoeveel km je per dag wil fietsen.", "Km per dag",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                numDay.Focus();
                return;
            }

            if (mTowns == null)
            {
                mPlanning = true;
                UpdateUiState();
                var progress = new Progress<string>(text => { lblStatus.Text = text; });
                var route = mRoute;
                try
                {
                    List<Town> towns;
                    if (UseGoogle)
                    {
                        string key = ApiKeyStore.Load();
                        if (string.IsNullOrEmpty(key))
                        {
                            key = AskApiKey();
                            if (string.IsNullOrEmpty(key)) return;
                        }
                        AddLog(string.Format("Logies en restaurants opzoeken via Google Places (±{0} zoekopdrachten)…",
                            GooglePlacesFinder.EstimateRequests(route.Points)));
                        towns = await GooglePlacesFinder.FindAsync(route.Points, key, progress, CancellationToken.None);
                    }
                    else
                    {
                        AddLog("Logies en restaurants langs de route opzoeken via OpenStreetMap (Overpass API)…");
                        towns = await LodgingFinder.FindAsync(route.Points, progress, CancellationToken.None);
                    }
                    if (route != mRoute)
                    {
                        return;   // intussen een andere route geopend
                    }
                    mTowns = towns;
                    int withLodging = towns.Count(t => t.Count(true) > 0);
                    int withFood = towns.Count(t => t.Restaurant > 0);
                    AddLog(string.Format("{0} plaatsen langs de route gevonden, {1} met logies, {2} met een restaurant.", towns.Count, withLodging, withFood));
                }
                catch (Exception ex)
                {
                    AddLog("Fout bij het opzoeken van logies: " + ex.Message);
                    if (UseGoogle)
                    {
                        AddLog("  (" + ex.GetType().Name + ") Laatste antwoord van Google: " + GooglePlacesFinder.DebugFile);
                    }
                    string hint = UseGoogle
                        ? "\n\nControleer je API-sleutel (knop 'Sleutel…') en of 'Places API (New)' aan staat."
                        : "\n\nProbeer het later opnieuw, of kies Google Places als bron.";
                    MessageBox.Show(this, "Logies opzoeken lukte niet:\n" + ex.Message + hint,
                        "Overnachtingen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    mPlanning = false;
                    UpdateUiState();
                }
            }

            RunPlan();
            RegenerateMarkers();
            tabs.SelectedTab = tabStages;
        }

        bool UseGoogle
        {
            get { return cbxSource.SelectedIndex == 1; }
        }

        private void CbxSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Andere bron = opnieuw opzoeken
            mTowns = null;
            if (mStages != null)
            {
                mStages = null;
                lvStages.Items.Clear();
                routePreview.SetTowns(null);
                RegenerateMarkers();
            }
            if (UseGoogle && string.IsNullOrEmpty(ApiKeyStore.Load()))
            {
                AskApiKey();
            }
            UpdateUiState();
        }

        private void BtnApiKey_Click(object sender, EventArgs e)
        {
            AskApiKey();
        }

        string AskApiKey()
        {
            using (var dialog = new ApiKeyDialog(ApiKeyStore.Load()))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }
                try
                {
                    ApiKeyStore.Save(dialog.ApiKey);
                    AddLog(string.IsNullOrEmpty(dialog.ApiKey) ? "Google API-sleutel verwijderd." : "Google API-sleutel opgeslagen.");
                    if (UseGoogle) mTowns = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Sleutel opslaan lukte niet: " + ex.Message, "API-sleutel",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return dialog.ApiKey;
            }
        }

        void RunPlan()
        {
            var settings = CurrentPlanSettings();
            var candidates = StagePlanner.Candidates(mTowns, mRoute.TotalDistance, settings);
            mStages = StagePlanner.Plan(mTowns, mRoute.TotalDistance, settings);
            routePreview.SetTowns(candidates);

            double offsetKm = (double)numOffset.Value;
            lvStages.BeginUpdate();
            lvStages.Items.Clear();
            var warnings = new List<string>();
            for (int i = 0; i < mStages.Count; i++)
            {
                var stage = mStages[i];
                bool last = i == mStages.Count - 1;
                var item = new ListViewItem(stage.Day.ToString());
                item.SubItems.Add(last ? "Finish" : stage.Name);
                item.SubItems.Add((stage.Length / 1000).ToString("N1") + " km");
                item.SubItems.Add((offsetKm + stage.EndDistance / 1000).ToString("N1"));
                item.SubItems.Add(stage.Town != null ? stage.Town.Count(settings.IncludeCamping) + "  (" + stage.Town.Summary(settings.IncludeCamping) + ")" : "");
                item.SubItems.Add(stage.Town != null ? stage.Town.FoodSummary : "");
                item.SubItems.Add(stage.Town != null ? (stage.Town.OffRoute / 1000).ToString("N1") + " km" : "");
                item.Tag = stage;
                if (StagePlanner.IsOutsideTolerance(stage, settings.DayKm, last))
                {
                    item.ForeColor = Color.FromArgb(185, 28, 28);
                    warnings.Add(string.Format("  Dag {0}: {1:N0} km", stage.Day, stage.Length / 1000));
                }
                lvStages.Items.Add(item);
            }
            lvStages.EndUpdate();

            AddLog(string.Format("Planning: {0} dagen rond {1:N0} km/dag, {2} geschikte plaatsen (min. {3} logies, min. {4} restaurant(s), max. {5:N1} km van de route).",
                mStages.Count, settings.DayKm, candidates.Count, settings.MinLodgings, settings.MinRestaurants, settings.MaxOffRouteKm));
            if (warnings.Count > 0)
            {
                AddLog("Geen geschikte plaats binnen ±" + (StagePlanner.Tolerance * 100).ToString("0") + "% voor:");
                warnings.ForEach(AddLog);
                AddLog("  Tip: verhoog 'Max. km van route', verlaag 'Min. aantal logies' / 'Min. restaurants' of vink campings aan.");
            }
        }

        IEnumerable<KmMarker> StageMarkers(double offsetKm)
        {
            bool camping = chkCamping.Checked;
            foreach (var stage in mStages)
            {
                if (stage.Town == null) continue;   // finish
                var town = stage.Town;
                yield return new KmMarker
                {
                    Kind = MarkerKind.DayEnd,
                    Day = stage.Day,
                    Lat = town.Lat,
                    Lon = town.Lon,
                    RouteDistance = stage.EndDistance,
                    LabelKm = offsetKm + stage.EndDistance / 1000,
                    Name = string.Format("Dag {0}: {1}", stage.Day, town.Name),
                    Description = string.Format("Overnachting dag {0}: {1}. Dagafstand {2:N0} km (km {3:N0}). Logies: {4}. Eten: {5}. {6:N1} km van de route.",
                        stage.Day, town.Name, stage.Length / 1000, offsetKm + stage.EndDistance / 1000,
                        town.Summary(camping), town.FoodSummary, town.OffRoute / 1000)
                };
            }
        }

        private void LvStages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvStages.SelectedItems.Count == 0) return;
            var stage = lvStages.SelectedItems[0].Tag as Stage;
            routePreview.SelectedMarker = mMarkers.FirstOrDefault(m => m.Kind == MarkerKind.DayEnd && stage != null && m.Day == stage.Day);
        }

        // ------------------------------------------------------------------ KML

        private void BtnKml_Click(object sender, EventArgs e)
        {
            if (mRoute == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "KML-bestanden (*.kml)|*.kml|Alle bestanden (*.*)|*.*";
                dialog.InitialDirectory = Path.GetDirectoryName(mRoute.FileName);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    AddLog("KML geopend: " + dialog.FileName);
                    mPlaces = KmlImporter.Load(dialog.FileName);
                    KmlImporter.LocateOnRoute(mPlaces, mRoute.Points);
                    double offsetKm = (double)numOffset.Value;

                    AddLog(string.Format("{0} plaatsen gevonden, gesorteerd volgens de route:", mPlaces.Count));
                    AddLog(string.Format("  {0,-32} {1,9} {2,9} {3,12}", "Plaats", "Km-stand", "Etappe", "Naast route"));
                    double previous = 0;
                    foreach (var place in mPlaces)
                    {
                        AddLog(string.Format("  {0,-32} {1,9:N1} {2,9:N1} {3,10:N0} m",
                            Truncate(place.Name, 32),
                            offsetKm + place.RouteDistance / 1000,
                            (place.RouteDistance - previous) / 1000,
                            place.OffRouteDistance));
                        previous = place.RouteDistance;
                    }
                    if (mPlaces.Count > 0)
                    {
                        AddLog(string.Format("  {0,-32} {1,9:N1} {2,9:N1}", "Finish",
                            offsetKm + mRoute.TotalDistance / 1000, (mRoute.TotalDistance - previous) / 1000));
                    }
                    AddLog("");

                    routePreview.SetPlaces(mPlaces);
                    tabs.SelectedTab = tabLog;
                }
                catch (Exception ex)
                {
                    AddLog("Fout bij het lezen van de KML: " + ex.Message);
                    MessageBox.Show(this, ex.Message, "Fout bij het lezen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        static string Truncate(string text, int length)
        {
            return text.Length <= length ? text : text.Substring(0, length - 1) + "…";
        }

        // ------------------------------------------------------------------ Hulpfuncties

        void UpdateUiState()
        {
            bool loaded = mRoute != null;
            btnWrite.Enabled = loaded;
            // Platte knop: zelf de kleuren zetten, anders is de uitgeschakelde tekst onleesbaar
            btnWrite.BackColor = loaded ? Color.FromArgb(37, 99, 235) : Color.FromArgb(203, 213, 225);
            btnWrite.ForeColor = loaded ? Color.White : Color.FromArgb(100, 116, 139);
            btnKml.Enabled = loaded;
            btnPlan.Enabled = loaded && !mPlanning;
            btnApiKey.Visible = UseGoogle;
            btnPlan.Text = mPlanning ? "Logies opzoeken…" : "Overnachtingsplaatsen plannen";
            chkRemoveOther.Enabled = rbSameFile.Checked;
            lblSeparateHint.Text = loaded
                ? "→ " + Path.GetFileName(GpxMarkerWriter.SeparateFileName(mRoute.FileName))
                : "→ <naam>-km.gpx naast de route";
            UpdateStatus();
        }

        void UpdateStatus()
        {
            if (mRoute == null)
            {
                lblStatus.Text = "Open een GPX-bestand om te beginnen";
                return;
            }
            int days = mMarkers.Count(m => m.Kind == MarkerKind.DayEnd);
            lblStatus.Text = string.Format("{0:N1} km  ·  {1:N0} trackpunten  ·  {2} markers{3}",
                mRoute.TotalDistance / 1000,
                mRoute.Points.Count,
                mMarkers.Count(m => m.Kind == MarkerKind.Interval),
                days > 0 ? string.Format("  ·  {0} dagen", days + 1) : "");
        }

        private void AddLog(string log)
        {
            edtLog.AppendText(log + Environment.NewLine);
        }
    }
}
