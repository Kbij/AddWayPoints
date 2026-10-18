namespace AddWayPoints
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.lnkOutput = new System.Windows.Forms.LinkLabel();
            this.btnKml = new System.Windows.Forms.Button();
            this.btnWrite = new System.Windows.Forms.Button();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.chkRemoveOther = new System.Windows.Forms.CheckBox();
            this.rbSameFile = new System.Windows.Forms.RadioButton();
            this.lblSeparateHint = new System.Windows.Forms.Label();
            this.rbSeparate = new System.Windows.Forms.RadioButton();
            this.grpPlan = new System.Windows.Forms.GroupBox();
            this.btnPlan = new System.Windows.Forms.Button();
            this.chkCamping = new System.Windows.Forms.CheckBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.cbxSource = new System.Windows.Forms.ComboBox();
            this.btnApiKey = new System.Windows.Forms.Button();
            this.numMinFood = new System.Windows.Forms.NumericUpDown();
            this.lblMinFood = new System.Windows.Forms.Label();
            this.numMinLodging = new System.Windows.Forms.NumericUpDown();
            this.lblMinLodging = new System.Windows.Forms.Label();
            this.numMaxOff = new System.Windows.Forms.NumericUpDown();
            this.lblMaxOff = new System.Windows.Forms.Label();
            this.grpMarkers = new System.Windows.Forms.GroupBox();
            this.lblNameHint = new System.Windows.Forms.Label();
            this.cbxSymbol = new System.Windows.Forms.ComboBox();
            this.lblSymbol = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.numDay = new System.Windows.Forms.NumericUpDown();
            this.lblDay = new System.Windows.Forms.Label();
            this.numOffset = new System.Windows.Forms.NumericUpDown();
            this.lblOffset = new System.Windows.Forms.Label();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.lblInterval = new System.Windows.Forms.Label();
            this.grpRoute = new System.Windows.Forms.GroupBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.chkSort = new System.Windows.Forms.CheckBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.routePreview = new AddWayPoints.RoutePreview();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabMarkers = new System.Windows.Forms.TabPage();
            this.lvMarkers = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colKm = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabStages = new System.Windows.Forms.TabPage();
            this.lvStages = new System.Windows.Forms.ListView();
            this.colDay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTown = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDayKm = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTotalKm = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLodging = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFood = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colOffRoute = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabLog = new System.Windows.Forms.TabPage();
            this.edtLog = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlSettings.SuspendLayout();
            this.grpOutput.SuspendLayout();
            this.grpPlan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinFood)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinLodging)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxOff)).BeginInit();
            this.grpMarkers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.grpRoute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabMarkers.SuspendLayout();
            this.tabStages.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // dlgOpenFile
            //
            this.dlgOpenFile.Filter = "GPX-bestanden (*.gpx)|*.gpx|Alle bestanden (*.*)|*.*";
            //
            // pnlSettings
            //
            this.pnlSettings.AutoScroll = true;
            this.pnlSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.pnlSettings.Controls.Add(this.lnkOutput);
            this.pnlSettings.Controls.Add(this.btnKml);
            this.pnlSettings.Controls.Add(this.btnWrite);
            this.pnlSettings.Controls.Add(this.grpOutput);
            this.pnlSettings.Controls.Add(this.grpPlan);
            this.pnlSettings.Controls.Add(this.grpMarkers);
            this.pnlSettings.Controls.Add(this.grpRoute);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSettings.Location = new System.Drawing.Point(0, 0);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(310, 778);
            this.pnlSettings.TabIndex = 0;
            //
            // lnkOutput
            //
            this.lnkOutput.Location = new System.Drawing.Point(12, 734);
            this.lnkOutput.Name = "lnkOutput";
            this.lnkOutput.Size = new System.Drawing.Size(280, 36);
            this.lnkOutput.TabIndex = 6;
            this.lnkOutput.Visible = false;
            this.lnkOutput.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkOutput_LinkClicked);
            //
            // btnKml
            //
            this.btnKml.Enabled = false;
            this.btnKml.Location = new System.Drawing.Point(12, 698);
            this.btnKml.Name = "btnKml";
            this.btnKml.Size = new System.Drawing.Size(280, 32);
            this.btnKml.TabIndex = 5;
            this.btnKml.Text = "Afstanden naar KML-plaatsen…";
            this.btnKml.UseVisualStyleBackColor = true;
            this.btnKml.Click += new System.EventHandler(this.BtnKml_Click);
            //
            // btnWrite
            //
            this.btnWrite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnWrite.Enabled = false;
            this.btnWrite.FlatAppearance.BorderSize = 0;
            this.btnWrite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWrite.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnWrite.ForeColor = System.Drawing.Color.White;
            this.btnWrite.Location = new System.Drawing.Point(12, 654);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(280, 40);
            this.btnWrite.TabIndex = 4;
            this.btnWrite.Text = "Markers schrijven";
            this.btnWrite.UseVisualStyleBackColor = false;
            this.btnWrite.Click += new System.EventHandler(this.BtnWrite_Click);
            //
            // grpOutput
            //
            this.grpOutput.Controls.Add(this.chkRemoveOther);
            this.grpOutput.Controls.Add(this.rbSameFile);
            this.grpOutput.Controls.Add(this.lblSeparateHint);
            this.grpOutput.Controls.Add(this.rbSeparate);
            this.grpOutput.Location = new System.Drawing.Point(12, 534);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(280, 112);
            this.grpOutput.TabIndex = 3;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "4. Uitvoer";
            //
            // chkRemoveOther
            //
            this.chkRemoveOther.AutoSize = true;
            this.chkRemoveOther.Enabled = false;
            this.chkRemoveOther.Location = new System.Drawing.Point(32, 84);
            this.chkRemoveOther.Name = "chkRemoveOther";
            this.chkRemoveOther.Size = new System.Drawing.Size(196, 19);
            this.chkRemoveOther.TabIndex = 3;
            this.chkRemoveOther.Text = "Ook andere waypoints wissen";
            this.chkRemoveOther.UseVisualStyleBackColor = true;
            //
            // rbSameFile
            //
            this.rbSameFile.AutoSize = true;
            this.rbSameFile.Location = new System.Drawing.Point(14, 62);
            this.rbSameFile.Name = "rbSameFile";
            this.rbSameFile.Size = new System.Drawing.Size(178, 19);
            this.rbSameFile.TabIndex = 2;
            this.rbSameFile.Text = "In het GPX-bestand zelf";
            this.rbSameFile.UseVisualStyleBackColor = true;
            this.rbSameFile.CheckedChanged += new System.EventHandler(this.OutputMode_Changed);
            //
            // lblSeparateHint
            //
            this.lblSeparateHint.AutoEllipsis = true;
            this.lblSeparateHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSeparateHint.Location = new System.Drawing.Point(32, 42);
            this.lblSeparateHint.Name = "lblSeparateHint";
            this.lblSeparateHint.Size = new System.Drawing.Size(240, 18);
            this.lblSeparateHint.TabIndex = 1;
            this.lblSeparateHint.Text = "→ <naam>-km.gpx naast de route";
            //
            // rbSeparate
            //
            this.rbSeparate.AutoSize = true;
            this.rbSeparate.Checked = true;
            this.rbSeparate.Location = new System.Drawing.Point(14, 22);
            this.rbSeparate.Name = "rbSeparate";
            this.rbSeparate.Size = new System.Drawing.Size(189, 19);
            this.rbSeparate.TabIndex = 0;
            this.rbSeparate.TabStop = true;
            this.rbSeparate.Text = "Apart bestand (eigen laag)";
            this.rbSeparate.UseVisualStyleBackColor = true;
            this.rbSeparate.CheckedChanged += new System.EventHandler(this.OutputMode_Changed);
            //
            // grpPlan
            //
            this.grpPlan.Controls.Add(this.btnPlan);
            this.grpPlan.Controls.Add(this.chkCamping);
            this.grpPlan.Controls.Add(this.lblSource);
            this.grpPlan.Controls.Add(this.cbxSource);
            this.grpPlan.Controls.Add(this.btnApiKey);
            this.grpPlan.Controls.Add(this.numMinFood);
            this.grpPlan.Controls.Add(this.lblMinFood);
            this.grpPlan.Controls.Add(this.numMinLodging);
            this.grpPlan.Controls.Add(this.lblMinLodging);
            this.grpPlan.Controls.Add(this.numMaxOff);
            this.grpPlan.Controls.Add(this.lblMaxOff);
            this.grpPlan.Controls.Add(this.numDay);
            this.grpPlan.Controls.Add(this.lblDay);
            this.grpPlan.Location = new System.Drawing.Point(12, 290);
            this.grpPlan.Name = "grpPlan";
            this.grpPlan.Size = new System.Drawing.Size(280, 238);
            this.grpPlan.TabIndex = 2;
            this.grpPlan.TabStop = false;
            this.grpPlan.Text = "3. Overnachtingen";
            //
            // btnPlan
            //
            this.btnPlan.Enabled = false;
            this.btnPlan.Location = new System.Drawing.Point(14, 198);
            this.btnPlan.Name = "btnPlan";
            this.btnPlan.Size = new System.Drawing.Size(252, 30);
            this.btnPlan.TabIndex = 11;
            this.btnPlan.Text = "Overnachtingsplaatsen plannen";
            this.btnPlan.UseVisualStyleBackColor = true;
            this.btnPlan.Click += new System.EventHandler(this.BtnPlan_Click);
            //
            // chkCamping
            //
            this.chkCamping.AutoSize = true;
            this.chkCamping.Location = new System.Drawing.Point(14, 144);
            this.chkCamping.Name = "chkCamping";
            this.chkCamping.Size = new System.Drawing.Size(150, 19);
            this.chkCamping.TabIndex = 7;
            this.chkCamping.Text = "Campings meetellen";
            this.chkCamping.UseVisualStyleBackColor = true;
            this.chkCamping.CheckedChanged += new System.EventHandler(this.PlanSettings_Changed);
            //
            // lblSource
            //
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(14, 171);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(37, 15);
            this.lblSource.TabIndex = 8;
            this.lblSource.Text = "Bron:";
            //
            // cbxSource
            //
            this.cbxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSource.FormattingEnabled = true;
            this.cbxSource.Items.AddRange(new object[] {
            "OpenStreetMap",
            "Google Places"});
            this.cbxSource.Location = new System.Drawing.Point(60, 168);
            this.cbxSource.Name = "cbxSource";
            this.cbxSource.Size = new System.Drawing.Size(130, 23);
            this.cbxSource.TabIndex = 9;
            this.cbxSource.SelectedIndexChanged += new System.EventHandler(this.CbxSource_SelectedIndexChanged);
            //
            // btnApiKey
            //
            this.btnApiKey.Location = new System.Drawing.Point(196, 167);
            this.btnApiKey.Name = "btnApiKey";
            this.btnApiKey.Size = new System.Drawing.Size(70, 25);
            this.btnApiKey.TabIndex = 10;
            this.btnApiKey.Text = "Sleutel…";
            this.btnApiKey.UseVisualStyleBackColor = true;
            this.btnApiKey.Click += new System.EventHandler(this.BtnApiKey_Click);
            //
            // numMinFood
            //
            this.numMinFood.Location = new System.Drawing.Point(150, 112);
            this.numMinFood.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMinFood.Name = "numMinFood";
            this.numMinFood.Size = new System.Drawing.Size(116, 23);
            this.numMinFood.TabIndex = 6;
            this.numMinFood.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numMinFood.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMinFood.ValueChanged += new System.EventHandler(this.PlanSettings_Changed);
            //
            // lblMinFood
            //
            this.lblMinFood.AutoSize = true;
            this.lblMinFood.Location = new System.Drawing.Point(14, 115);
            this.lblMinFood.Name = "lblMinFood";
            this.lblMinFood.Size = new System.Drawing.Size(100, 15);
            this.lblMinFood.TabIndex = 5;
            this.lblMinFood.Text = "Min. restaurants:";
            //
            // numMinLodging
            //
            this.numMinLodging.Location = new System.Drawing.Point(150, 82);
            this.numMinLodging.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMinLodging.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMinLodging.Name = "numMinLodging";
            this.numMinLodging.Size = new System.Drawing.Size(116, 23);
            this.numMinLodging.TabIndex = 5;
            this.numMinLodging.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numMinLodging.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numMinLodging.ValueChanged += new System.EventHandler(this.PlanSettings_Changed);
            //
            // lblMinLodging
            //
            this.lblMinLodging.AutoSize = true;
            this.lblMinLodging.Location = new System.Drawing.Point(14, 85);
            this.lblMinLodging.Name = "lblMinLodging";
            this.lblMinLodging.Size = new System.Drawing.Size(110, 15);
            this.lblMinLodging.TabIndex = 4;
            this.lblMinLodging.Text = "Min. aantal logies:";
            //
            // numMaxOff
            //
            this.numMaxOff.DecimalPlaces = 1;
            this.numMaxOff.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numMaxOff.Location = new System.Drawing.Point(150, 52);
            this.numMaxOff.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numMaxOff.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numMaxOff.Name = "numMaxOff";
            this.numMaxOff.Size = new System.Drawing.Size(116, 23);
            this.numMaxOff.TabIndex = 3;
            this.numMaxOff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numMaxOff.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numMaxOff.ValueChanged += new System.EventHandler(this.PlanSettings_Changed);
            //
            // lblMaxOff
            //
            this.lblMaxOff.AutoSize = true;
            this.lblMaxOff.Location = new System.Drawing.Point(14, 55);
            this.lblMaxOff.Name = "lblMaxOff";
            this.lblMaxOff.Size = new System.Drawing.Size(118, 15);
            this.lblMaxOff.TabIndex = 2;
            this.lblMaxOff.Text = "Max. km van route:";
            //
            // grpMarkers
            //
            this.grpMarkers.Controls.Add(this.lblNameHint);
            this.grpMarkers.Controls.Add(this.cbxSymbol);
            this.grpMarkers.Controls.Add(this.lblSymbol);
            this.grpMarkers.Controls.Add(this.txtName);
            this.grpMarkers.Controls.Add(this.lblName);
            this.grpMarkers.Controls.Add(this.numOffset);
            this.grpMarkers.Controls.Add(this.lblOffset);
            this.grpMarkers.Controls.Add(this.numInterval);
            this.grpMarkers.Controls.Add(this.lblInterval);
            this.grpMarkers.Location = new System.Drawing.Point(12, 118);
            this.grpMarkers.Name = "grpMarkers";
            this.grpMarkers.Size = new System.Drawing.Size(280, 166);
            this.grpMarkers.TabIndex = 1;
            this.grpMarkers.TabStop = false;
            this.grpMarkers.Text = "2. Markers";
            //
            // lblNameHint
            //
            this.lblNameHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblNameHint.Location = new System.Drawing.Point(14, 142);
            this.lblNameHint.Name = "lblNameHint";
            this.lblNameHint.Size = new System.Drawing.Size(258, 20);
            this.lblNameHint.TabIndex = 10;
            this.lblNameHint.Text = "{km} in de naam = kilometerstand";
            //
            // cbxSymbol
            //
            this.cbxSymbol.FormattingEnabled = true;
            this.cbxSymbol.Items.AddRange(new object[] {
            "Flag, Blue",
            "Flag, Red",
            "Flag, Green",
            "Pin, Blue",
            "Waypoint",
            "Mile Marker",
            "Information"});
            this.cbxSymbol.Location = new System.Drawing.Point(150, 112);
            this.cbxSymbol.Name = "cbxSymbol";
            this.cbxSymbol.Size = new System.Drawing.Size(116, 23);
            this.cbxSymbol.TabIndex = 9;
            //
            // lblSymbol
            //
            this.lblSymbol.AutoSize = true;
            this.lblSymbol.Location = new System.Drawing.Point(14, 115);
            this.lblSymbol.Name = "lblSymbol";
            this.lblSymbol.Size = new System.Drawing.Size(84, 15);
            this.lblSymbol.TabIndex = 8;
            this.lblSymbol.Text = "Symbool (sym):";
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(150, 82);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(116, 23);
            this.txtName.TabIndex = 7;
            this.txtName.Text = "{km}";
            this.txtName.TextChanged += new System.EventHandler(this.Settings_Changed);
            //
            // lblName
            //
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 85);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(42, 15);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "Naam:";
            //
            // numDay
            //
            this.numDay.Location = new System.Drawing.Point(150, 22);
            this.numDay.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numDay.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numDay.Name = "numDay";
            this.numDay.Size = new System.Drawing.Size(116, 23);
            this.numDay.TabIndex = 5;
            this.numDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numDay.ValueChanged += new System.EventHandler(this.PlanSettings_Changed);
            //
            // lblDay
            //
            this.lblDay.AutoSize = true;
            this.lblDay.Location = new System.Drawing.Point(14, 25);
            this.lblDay.Name = "lblDay";
            this.lblDay.Size = new System.Drawing.Size(131, 15);
            this.lblDay.TabIndex = 4;
            this.lblDay.Text = "Km per dag:";
            //
            // numOffset
            //
            this.numOffset.DecimalPlaces = 1;
            this.numOffset.Location = new System.Drawing.Point(150, 52);
            this.numOffset.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numOffset.Name = "numOffset";
            this.numOffset.Size = new System.Drawing.Size(116, 23);
            this.numOffset.TabIndex = 3;
            this.numOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numOffset.ValueChanged += new System.EventHandler(this.Settings_Changed);
            //
            // lblOffset
            //
            this.lblOffset.AutoSize = true;
            this.lblOffset.Location = new System.Drawing.Point(14, 55);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(103, 15);
            this.lblOffset.TabIndex = 2;
            this.lblOffset.Text = "Start-km (offset):";
            //
            // numInterval
            //
            this.numInterval.DecimalPlaces = 1;
            this.numInterval.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numInterval.Location = new System.Drawing.Point(150, 22);
            this.numInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(116, 23);
            this.numInterval.TabIndex = 1;
            this.numInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numInterval.ValueChanged += new System.EventHandler(this.Settings_Changed);
            //
            // lblInterval
            //
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(14, 25);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(85, 15);
            this.lblInterval.TabIndex = 0;
            this.lblInterval.Text = "Marker om de km:";
            //
            // grpRoute
            //
            this.grpRoute.Controls.Add(this.lblFile);
            this.grpRoute.Controls.Add(this.chkSort);
            this.grpRoute.Controls.Add(this.btnOpen);
            this.grpRoute.Location = new System.Drawing.Point(12, 8);
            this.grpRoute.Name = "grpRoute";
            this.grpRoute.Size = new System.Drawing.Size(280, 104);
            this.grpRoute.TabIndex = 0;
            this.grpRoute.TabStop = false;
            this.grpRoute.Text = "1. Route";
            //
            // lblFile
            //
            this.lblFile.AutoEllipsis = true;
            this.lblFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblFile.Location = new System.Drawing.Point(14, 80);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(258, 20);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "Nog geen bestand geladen";
            //
            // chkSort
            //
            this.chkSort.AutoSize = true;
            this.chkSort.Location = new System.Drawing.Point(14, 58);
            this.chkSort.Name = "chkSort";
            this.chkSort.Size = new System.Drawing.Size(232, 19);
            this.chkSort.TabIndex = 1;
            this.chkSort.Text = "Losse tracks sorteren en samenvoegen";
            this.chkSort.UseVisualStyleBackColor = true;
            //
            // btnOpen
            //
            this.btnOpen.Location = new System.Drawing.Point(14, 22);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(252, 32);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "GPX-bestand openen…";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            //
            // splitMain
            //
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(310, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.routePreview);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.tabs);
            this.splitMain.Size = new System.Drawing.Size(790, 778);
            this.splitMain.SplitterDistance = 430;
            this.splitMain.TabIndex = 1;
            //
            // routePreview
            //
            this.routePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.routePreview.Location = new System.Drawing.Point(0, 0);
            this.routePreview.Name = "routePreview";
            this.routePreview.Size = new System.Drawing.Size(790, 430);
            this.routePreview.TabIndex = 0;
            //
            // tabs
            //
            this.tabs.Controls.Add(this.tabMarkers);
            this.tabs.Controls.Add(this.tabStages);
            this.tabs.Controls.Add(this.tabLog);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(790, 244);
            this.tabs.TabIndex = 0;
            //
            // tabMarkers
            //
            this.tabMarkers.Controls.Add(this.lvMarkers);
            this.tabMarkers.Location = new System.Drawing.Point(4, 24);
            this.tabMarkers.Name = "tabMarkers";
            this.tabMarkers.Size = new System.Drawing.Size(782, 216);
            this.tabMarkers.TabIndex = 0;
            this.tabMarkers.Text = "Markers";
            this.tabMarkers.UseVisualStyleBackColor = true;
            //
            // lvMarkers
            //
            this.lvMarkers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvMarkers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colKm,
            this.colStage,
            this.colLat,
            this.colLon});
            this.lvMarkers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMarkers.FullRowSelect = true;
            this.lvMarkers.GridLines = true;
            this.lvMarkers.HideSelection = false;
            this.lvMarkers.Location = new System.Drawing.Point(0, 0);
            this.lvMarkers.MultiSelect = false;
            this.lvMarkers.Name = "lvMarkers";
            this.lvMarkers.Size = new System.Drawing.Size(782, 216);
            this.lvMarkers.TabIndex = 0;
            this.lvMarkers.UseCompatibleStateImageBehavior = false;
            this.lvMarkers.View = System.Windows.Forms.View.Details;
            this.lvMarkers.SelectedIndexChanged += new System.EventHandler(this.LvMarkers_SelectedIndexChanged);
            //
            // colName
            //
            this.colName.Text = "Naam";
            this.colName.Width = 160;
            //
            // colKm
            //
            this.colKm.Text = "Km-stand";
            this.colKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colKm.Width = 90;
            //
            // colStage
            //
            this.colStage.Text = "Vanaf start";
            this.colStage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colStage.Width = 90;
            //
            // colLat
            //
            this.colLat.Text = "Lat";
            this.colLat.Width = 110;
            //
            // colLon
            //
            this.colLon.Text = "Lon";
            this.colLon.Width = 110;
            //
            // tabStages
            //
            this.tabStages.Controls.Add(this.lvStages);
            this.tabStages.Location = new System.Drawing.Point(4, 24);
            this.tabStages.Name = "tabStages";
            this.tabStages.Size = new System.Drawing.Size(782, 256);
            this.tabStages.TabIndex = 1;
            this.tabStages.Text = "Etappes";
            this.tabStages.UseVisualStyleBackColor = true;
            //
            // lvStages
            //
            this.lvStages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvStages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDay,
            this.colTown,
            this.colDayKm,
            this.colTotalKm,
            this.colLodging,
            this.colFood,
            this.colOffRoute});
            this.lvStages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvStages.FullRowSelect = true;
            this.lvStages.GridLines = true;
            this.lvStages.HideSelection = false;
            this.lvStages.Location = new System.Drawing.Point(0, 0);
            this.lvStages.MultiSelect = false;
            this.lvStages.Name = "lvStages";
            this.lvStages.Size = new System.Drawing.Size(782, 256);
            this.lvStages.TabIndex = 0;
            this.lvStages.UseCompatibleStateImageBehavior = false;
            this.lvStages.View = System.Windows.Forms.View.Details;
            this.lvStages.SelectedIndexChanged += new System.EventHandler(this.LvStages_SelectedIndexChanged);
            //
            // colDay
            //
            this.colDay.Text = "Dag";
            this.colDay.Width = 45;
            //
            // colTown
            //
            this.colTown.Text = "Overnachting";
            this.colTown.Width = 150;
            //
            // colDayKm
            //
            this.colDayKm.Text = "Dagafstand";
            this.colDayKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colDayKm.Width = 85;
            //
            // colTotalKm
            //
            this.colTotalKm.Text = "Km-stand";
            this.colTotalKm.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colTotalKm.Width = 80;
            //
            // colLodging
            //
            this.colLodging.Text = "Logies";
            this.colLodging.Width = 180;
            //
            // colFood
            //
            this.colFood.Text = "Eten";
            this.colFood.Width = 140;
            //
            // colOffRoute
            //
            this.colOffRoute.Text = "Van route";
            this.colOffRoute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.colOffRoute.Width = 80;
            //
            // tabLog
            //
            this.tabLog.Controls.Add(this.edtLog);
            this.tabLog.Location = new System.Drawing.Point(4, 24);
            this.tabLog.Name = "tabLog";
            this.tabLog.Size = new System.Drawing.Size(782, 216);
            this.tabLog.TabIndex = 2;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            //
            // edtLog
            //
            this.edtLog.BackColor = System.Drawing.Color.White;
            this.edtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.edtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.edtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.edtLog.Location = new System.Drawing.Point(0, 0);
            this.edtLog.Multiline = true;
            this.edtLog.Name = "edtLog";
            this.edtLog.ReadOnly = true;
            this.edtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.edtLog.Size = new System.Drawing.Size(782, 216);
            this.edtLog.TabIndex = 0;
            this.edtLog.WordWrap = false;
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 778);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip.TabIndex = 2;
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1085, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "Klaar";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 800);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Km-markers voor GPX-routes";
            this.pnlSettings.ResumeLayout(false);
            this.grpOutput.ResumeLayout(false);
            this.grpOutput.PerformLayout();
            this.grpPlan.ResumeLayout(false);
            this.grpPlan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinFood)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinLodging)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxOff)).EndInit();
            this.grpMarkers.ResumeLayout(false);
            this.grpMarkers.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.grpRoute.ResumeLayout(false);
            this.grpRoute.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.tabMarkers.ResumeLayout(false);
            this.tabStages.ResumeLayout(false);
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.GroupBox grpRoute;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.CheckBox chkSort;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.GroupBox grpMarkers;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.NumericUpDown numOffset;
        private System.Windows.Forms.Label lblDay;
        private System.Windows.Forms.NumericUpDown numDay;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.ComboBox cbxSymbol;
        private System.Windows.Forms.Label lblNameHint;
        private System.Windows.Forms.GroupBox grpPlan;
        private System.Windows.Forms.Label lblMaxOff;
        private System.Windows.Forms.NumericUpDown numMaxOff;
        private System.Windows.Forms.Label lblMinLodging;
        private System.Windows.Forms.NumericUpDown numMinLodging;
        private System.Windows.Forms.Label lblMinFood;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.ComboBox cbxSource;
        private System.Windows.Forms.Button btnApiKey;
        private System.Windows.Forms.NumericUpDown numMinFood;
        private System.Windows.Forms.CheckBox chkCamping;
        private System.Windows.Forms.Button btnPlan;
        private System.Windows.Forms.TabPage tabStages;
        private System.Windows.Forms.ListView lvStages;
        private System.Windows.Forms.ColumnHeader colDay;
        private System.Windows.Forms.ColumnHeader colTown;
        private System.Windows.Forms.ColumnHeader colDayKm;
        private System.Windows.Forms.ColumnHeader colTotalKm;
        private System.Windows.Forms.ColumnHeader colLodging;
        private System.Windows.Forms.ColumnHeader colFood;
        private System.Windows.Forms.ColumnHeader colOffRoute;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.RadioButton rbSeparate;
        private System.Windows.Forms.Label lblSeparateHint;
        private System.Windows.Forms.RadioButton rbSameFile;
        private System.Windows.Forms.CheckBox chkRemoveOther;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Button btnKml;
        private System.Windows.Forms.LinkLabel lnkOutput;
        private System.Windows.Forms.SplitContainer splitMain;
        private RoutePreview routePreview;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabMarkers;
        private System.Windows.Forms.ListView lvMarkers;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colKm;
        private System.Windows.Forms.ColumnHeader colStage;
        private System.Windows.Forms.ColumnHeader colLat;
        private System.Windows.Forms.ColumnHeader colLon;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.TextBox edtLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
