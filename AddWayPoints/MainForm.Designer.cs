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
            this.btnSelect = new System.Windows.Forms.Button();
            this.edtLog = new System.Windows.Forms.TextBox();
            this.edtDistance = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnWriteWaypoints = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.edtOffset = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.FileName = "*.gpx";
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(12, 24);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(103, 33);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "OpenFile";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // edtLog
            // 
            this.edtLog.AcceptsReturn = true;
            this.edtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtLog.Location = new System.Drawing.Point(12, 71);
            this.edtLog.Multiline = true;
            this.edtLog.Name = "edtLog";
            this.edtLog.ReadOnly = true;
            this.edtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.edtLog.Size = new System.Drawing.Size(776, 367);
            this.edtLog.TabIndex = 2;
            // 
            // edtDistance
            // 
            this.edtDistance.Location = new System.Drawing.Point(233, 29);
            this.edtDistance.Name = "edtDistance";
            this.edtDistance.Size = new System.Drawing.Size(100, 22);
            this.edtDistance.TabIndex = 3;
            this.edtDistance.Text = "10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(230, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "WP Distance (km):";
            // 
            // btnWriteWaypoints
            // 
            this.btnWriteWaypoints.Enabled = false;
            this.btnWriteWaypoints.Location = new System.Drawing.Point(511, 22);
            this.btnWriteWaypoints.Name = "btnWriteWaypoints";
            this.btnWriteWaypoints.Size = new System.Drawing.Size(144, 33);
            this.btnWriteWaypoints.TabIndex = 5;
            this.btnWriteWaypoints.Text = "Write Waypoints";
            this.btnWriteWaypoints.UseVisualStyleBackColor = true;
            this.btnWriteWaypoints.Click += new System.EventHandler(this.BtnWriteWaypoints_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.AutoSize = true;
            this.btnRemove.Location = new System.Drawing.Point(361, 29);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(134, 21);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.Text = "Remove Existing";
            this.btnRemove.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(139, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Offset (km):";
            // 
            // edtOffset
            // 
            this.edtOffset.Location = new System.Drawing.Point(142, 29);
            this.edtOffset.Name = "edtOffset";
            this.edtOffset.Size = new System.Drawing.Size(79, 22);
            this.edtOffset.TabIndex = 7;
            this.edtOffset.Text = "0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(662, 24);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 28);
            this.button1.TabIndex = 9;
            this.button1.Text = "KML Dist";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.edtOffset);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnWriteWaypoints);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.edtDistance);
            this.Controls.Add(this.edtLog);
            this.Controls.Add(this.btnSelect);
            this.Name = "MainForm";
            this.Text = "GPS Waypoints";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox edtLog;
        private System.Windows.Forms.TextBox edtDistance;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnWriteWaypoints;
        private System.Windows.Forms.CheckBox btnRemove;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox edtOffset;
        private System.Windows.Forms.Button button1;
    }
}

