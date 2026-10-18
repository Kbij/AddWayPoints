using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AddWayPoints
{
    /// <summary>Klein venster om de Google API-sleutel in te geven.</summary>
    internal class ApiKeyDialog : Form
    {
        const string ConsoleUrl = "https://console.cloud.google.com/google/maps-apis/credentials";
        const string EnableUrl = "https://console.cloud.google.com/apis/library/places.googleapis.com";

        readonly TextBox mKey;

        public ApiKeyDialog(string currentKey)
        {
            Text = "Google Places API-sleutel";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(7F, 15F);
            ClientSize = new Size(480, 250);

            var info = new Label
            {
                Location = new Point(14, 12),
                Size = new Size(452, 110),
                Text = "Zo maak je een sleutel aan:\r\n" +
                       "1. Maak (of kies) een project in Google Cloud en koppel een betaalaccount.\r\n" +
                       "2. Zet 'Places API (New)' aan.\r\n" +
                       "3. Maak bij 'Credentials' een API-sleutel en beperk die best tot 'Places API (New)'.\r\n\r\n" +
                       "De eerste 5.000 zoekopdrachten per maand zijn gratis; één planning van 750 km gebruikt er ±300."
            };

            var linkEnable = new LinkLabel { Location = new Point(14, 124), AutoSize = true, Text = "Places API (New) aanzetten" };
            linkEnable.LinkClicked += (s, e) => Open(EnableUrl);
            var linkConsole = new LinkLabel { Location = new Point(220, 124), AutoSize = true, Text = "API-sleutel aanmaken" };
            linkConsole.LinkClicked += (s, e) => Open(ConsoleUrl);

            var label = new Label { Location = new Point(14, 158), AutoSize = true, Text = "API-sleutel:" };
            mKey = new TextBox
            {
                Location = new Point(14, 178),
                Size = new Size(452, 23),
                Text = currentKey ?? "",
                UseSystemPasswordChar = true
            };
            var show = new CheckBox { Location = new Point(14, 210), AutoSize = true, Text = "Tonen" };
            show.CheckedChanged += (s, e) => mKey.UseSystemPasswordChar = !show.Checked;

            var ok = new Button { Text = "Opslaan", DialogResult = DialogResult.OK, Location = new Point(290, 210), Size = new Size(85, 28) };
            var cancel = new Button { Text = "Annuleren", DialogResult = DialogResult.Cancel, Location = new Point(381, 210), Size = new Size(85, 28) };
            AcceptButton = ok;
            CancelButton = cancel;

            Controls.AddRange(new Control[] { info, linkEnable, linkConsole, label, mKey, show, ok, cancel });
        }

        public string ApiKey
        {
            get { return mKey.Text.Trim(); }
        }

        static void Open(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                Clipboard.SetText(url);
                MessageBox.Show("Kon de browser niet openen; de link staat op het klembord.");
            }
        }
    }
}
