using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace HarekatMerkezi
{
    public partial class Form1 : Form
    {
        private UdpClient udpServer;
        private bool isListening = false;

        private GMapControl gmap;
        private Panel dashboardPanel;
        private Label lblTelemetry;
        private GMapOverlay targetOverlay;

        private Timer animTimer;
        private int pulseRadius = 0;
        private PointLatLng? decoyPos = null;
        private PointLatLng? realPos = null;
        private PointLatLng hqPos = new PointLatLng(39.9666, 32.7666); // Aselsan Karargah

        private string dbPath = "HarekatVeritabani_Faz2.db";

        public Form1()
        {
            InitializeComponent();
            SetupRadarUI();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE TacticalLogs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SwarmId INTEGER,
                            Latitude REAL,
                            Longitude REAL,
                            Thermal REAL,
                            IsDecoy INTEGER,
                            LogTime TEXT
                        )";
                    using (var command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void SetupRadarUI()
        {
            this.Text = "STANAG 4609 NATO Taktik C2 (Eksiksiz Sürüm)";
            this.Size = new Size(1100, 750); // Ekranı biraz daha büyüttük
            this.BackColor = Color.FromArgb(15, 15, 15);

            gmap = new GMapControl();
            gmap.Dock = DockStyle.Fill;
            gmap.MapProvider = GMapProviders.GoogleSatelliteMap;
            gmap.Position = hqPos;
            gmap.MinZoom = 5;
            gmap.MaxZoom = 18;
            gmap.Zoom = 8;
            gmap.ShowCenter = false;
            gmap.Paint += Gmap_Paint;
            this.Controls.Add(gmap);

            targetOverlay = new GMapOverlay("targets");
            gmap.Overlays.Add(targetOverlay);

            GMarkerGoogle hqMarker = new GMarkerGoogle(hqPos, GMarkerGoogleType.green_dot);
            hqMarker.ToolTipText = "KARARGAH (C2 MERKEZİ)";
            targetOverlay.Markers.Add(hqMarker);

            dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Right;
            dashboardPanel.Width = 380; // Paneli de genişlettik
            dashboardPanel.BackColor = Color.FromArgb(25, 25, 25);
            this.Controls.Add(dashboardPanel);

            Label lblHeader = new Label();
            lblHeader.Text = "OVRK TELEMETRİ & SENSÖR FÜZYONU";
            lblHeader.ForeColor = Color.Cyan;
            lblHeader.Font = new Font("Consolas", 11, FontStyle.Bold);
            lblHeader.Dock = DockStyle.Top;
            lblHeader.Height = 50;
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;
            dashboardPanel.Controls.Add(lblHeader);

            lblTelemetry = new Label();
            lblTelemetry.ForeColor = Color.Lime;
            lblTelemetry.Font = new Font("Consolas", 10, FontStyle.Regular);
            lblTelemetry.Dock = DockStyle.Top;
            lblTelemetry.Height = 300; // Yazıların sığması için uzatıldı
            lblTelemetry.Text = "\nSistem Beklemede...";
            dashboardPanel.Controls.Add(lblTelemetry);

            listBox1.Dock = DockStyle.Bottom;
            listBox1.Height = 150;
            listBox1.BackColor = Color.Black;
            listBox1.ForeColor = Color.DarkOrange;

            animTimer = new Timer();
            animTimer.Interval = 40;
            animTimer.Tick += AnimTimer_Tick;
            animTimer.Start();
        }

        // ========================================================
        // ASKERİ HAVERSINE MESAFE VE YÖN HESAPLAMA ALGORİTMASI
        // ========================================================
        private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Dünya yarıçapı
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private string GetDirection(double hqLat, double hqLon, double tLat, double tLon)
        {
            string ns = tLat >= hqLat ? "Kuzey" : "Güney";
            string ew = tLon >= hqLon ? "Doğu" : "Batı";
            return $"{ns}-{ew}";
        }
        // ========================================================

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            pulseRadius += 3;
            if (pulseRadius > 150) pulseRadius = 0;
            gmap.Invalidate();
        }

        private void Gmap_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (decoyPos.HasValue)
            {
                GPoint pt = gmap.FromLatLngToLocal(decoyPos.Value);
                Pen p = new Pen(Color.FromArgb(150, Color.Orange), 2);
                g.DrawEllipse(p, (int)pt.X - pulseRadius, (int)pt.Y - pulseRadius, pulseRadius * 2, pulseRadius * 2);
            }

            if (realPos.HasValue)
            {
                GPoint pt = gmap.FromLatLngToLocal(realPos.Value);
                Pen p = new Pen(Color.FromArgb(200, Color.Red), 4);
                g.DrawEllipse(p, (int)pt.X - pulseRadius, (int)pt.Y - pulseRadius, pulseRadius * 2, pulseRadius * 2);

                GPoint hqPt = gmap.FromLatLngToLocal(hqPos);
                Pen laserPen = new Pen(Color.Yellow, 3);
                laserPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(laserPen, (int)hqPt.X, (int)hqPt.Y, (int)pt.X, (int)pt.Y);
            }
        }

        private void Form1_Load(object sender, EventArgs e) { StartListening(); }

        private async void StartListening()
        {
            udpServer = new UdpClient(14530);
            isListening = true;
            listBox1.Items.Add("[C2 MERKEZİ] Kriptolu Taktik Ağ Dinleniyor...");

            while (isListening)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                byte[] receivedBytes = result.Buffer;

                byte secretKey = 0xAA;
                for (int i = 0; i < receivedBytes.Length; i++) { receivedBytes[i] ^= secretKey; }

                if (receivedBytes.Length >= 52)
                {
                    byte swarmId = receivedBytes[4];
                    double lat = BitConverter.ToDouble(receivedBytes, 5);
                    double lon = BitConverter.ToDouble(receivedBytes, 13);
                    float alt = BitConverter.ToSingle(receivedBytes, 21);
                    float speed = BitConverter.ToSingle(receivedBytes, 25);
                    float aiConf = BitConverter.ToSingle(receivedBytes, 42);
                    bool laserActive = BitConverter.ToBoolean(receivedBytes, 46);
                    float thermal = BitConverter.ToSingle(receivedBytes, 47);
                    bool isDecoy = BitConverter.ToBoolean(receivedBytes, 51);

                    string swarmName = swarmId == 1 ? "ALFA" : (swarmId == 2 ? "BRAVO" : "CHARLIE");

                    this.Invoke((MethodInvoker)delegate {
                        UpdateMapAndDashboard(lat, lon, swarmName, aiConf, laserActive, thermal, isDecoy, alt, speed);
                        LogToDatabase(swarmId, lat, lon, thermal, isDecoy);
                    });
                }
            }
        }

        private void UpdateMapAndDashboard(double lat, double lon, string swarmName, float aiConf, bool laser, float thermal, bool isDecoy, float alt, float speed)
        {
            // Mesafe ve Yön Hesaplama
            double distKm = CalculateDistanceKm(hqPos.Lat, hqPos.Lng, lat, lon);
            string direction = GetDirection(hqPos.Lat, hqPos.Lng, lat, lon);

            lblTelemetry.Text = $"\n--- OVRK {swarmName} CANLI VERİ ---\n\n" +
                                $"Uydu Konumu : {lat:F4}, {lon:F4}\n" +
                                $"Uzaklık     : {distKm:F1} km\n" +
                                $"Hedef Yönü  : {direction}\n" +
                                $"İrtifa      : {alt} m\n" +
                                $"Hız         : {speed} knot\n" +
                                $"Termal İzi  : {thermal} °C\n" +
                                $"Yapay Zeka  : %{aiConf} Eşleşme\n\n" +
                                $"Hedef Durumu: {(isDecoy ? "SAHTE (DECOY)" : "GERÇEK S-400")}\n" +
                                $"Lazer Durumu: {(laser ? "AKTİF KİLİT" : "KAPALI")}";

            if (isDecoy) lblTelemetry.ForeColor = Color.Yellow;
            else if (laser) lblTelemetry.ForeColor = Color.Red;
            else lblTelemetry.ForeColor = Color.Lime;

            string logMsg = $"[{DateTime.Now.ToString("HH:mm:ss")}] [OVRK {swarmName}] {distKm:F1} km Uzakta - Isı: {thermal}C";
            listBox1.Items.Add(logMsg);
            listBox1.TopIndex = listBox1.Items.Count - 1;

            PointLatLng targetPos = new PointLatLng(lat, lon);

            if (isDecoy)
            {
                decoyPos = targetPos;
            }
            else
            {
                realPos = targetPos;
                gmap.Position = new PointLatLng((hqPos.Lat + realPos.Value.Lat) / 2, (hqPos.Lng + realPos.Value.Lng) / 2);
            }

            GMarkerGoogle targetMarker = new GMarkerGoogle(targetPos, isDecoy ? GMarkerGoogleType.yellow_dot : GMarkerGoogleType.red_dot);
            targetMarker.ToolTipText = isDecoy ? $"SAHTE HEDEF\nMesafe: {distKm:F1} km\nIsı: {thermal}C" : $"GERÇEK S-400\nMesafe: {distKm:F1} km\nIsı: {thermal}C";
            targetOverlay.Markers.Add(targetMarker);
        }

        private void LogToDatabase(byte swarmId, double lat, double lon, float thermal, bool isDecoy)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO TacticalLogs (SwarmId, Latitude, Longitude, Thermal, IsDecoy, LogTime) VALUES (@sId, @lat, @lon, @therm, @decoy, @time)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sId", swarmId);
                        command.Parameters.AddWithValue("@lat", lat);
                        command.Parameters.AddWithValue("@lon", lon);
                        command.Parameters.AddWithValue("@therm", thermal);
                        command.Parameters.AddWithValue("@decoy", isDecoy ? 1 : 0);
                        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception) { }
        }
    }
}