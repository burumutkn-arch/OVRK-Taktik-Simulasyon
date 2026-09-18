using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace HarekatMerkezi
{
    public partial class Form1 : Form
    {
        private UdpClient udpServer;
        private bool isListening = false;
        private Panel radarPanel;

        private double targetLat = 0;
        private double targetLon = 0;
        private bool targetFound = false;
        private bool laserPainted = false;

        private string dbPath = "HarekatVeritabani_V2.db";

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
                            DeviceId INTEGER,
                            SwarmId INTEGER,
                            Latitude REAL,
                            Longitude REAL,
                            AiConfidence REAL,
                            LogTime TEXT
                        )";
                    using (var command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void LogThreatToDatabase(uint deviceId, byte swarmId, double lat, double lon, float aiConf)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO TacticalLogs (DeviceId, SwarmId, Latitude, Longitude, AiConfidence, LogTime) VALUES (@devId, @swarmId, @lat, @lon, @aiConf, @time)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@devId", deviceId);
                        command.Parameters.AddWithValue("@swarmId", swarmId);
                        command.Parameters.AddWithValue("@lat", lat);
                        command.Parameters.AddWithValue("@lon", lon);
                        command.Parameters.AddWithValue("@aiConf", aiConf);
                        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception) { }
        }

        private void SetupRadarUI()
        {
            this.Text = "ASELSAN Ağ Merkezli Harp (OVRK Sürü & Lazer Taktik Ekranı)";
            this.Size = new Size(800, 650);
            this.BackColor = Color.FromArgb(20, 20, 20);

            listBox1.Dock = DockStyle.Bottom;
            listBox1.Height = 150;
            listBox1.BackColor = Color.Black;
            listBox1.ForeColor = Color.Lime;
            listBox1.Font = new Font("Consolas", 10, FontStyle.Regular);

            radarPanel = new Panel();
            radarPanel.Dock = DockStyle.Fill;
            radarPanel.BackColor = Color.Black;
            radarPanel.Paint += RadarPanel_Paint;
            this.Controls.Add(radarPanel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartListening();
        }

        private async void StartListening()
        {
            udpServer = new UdpClient(14530);
            isListening = true;
            listBox1.Items.Add("[HAREKAT MERKEZI] Taktik Sürü Agi Dinleniyor...");

            while (isListening)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                byte[] receivedBytes = result.Buffer;

                byte secretKey = 0xAA;
                for (int i = 0; i < receivedBytes.Length; i++) { receivedBytes[i] ^= secretKey; }

                if (receivedBytes.Length >= 47)
                {
                    uint deviceId = BitConverter.ToUInt32(receivedBytes, 0);
                    byte swarmId = receivedBytes[4];
                    targetLat = BitConverter.ToDouble(receivedBytes, 5);
                    targetLon = BitConverter.ToDouble(receivedBytes, 13);
                    float aiConfidence = BitConverter.ToSingle(receivedBytes, 42);
                    bool laserActive = BitConverter.ToBoolean(receivedBytes, 46);

                    string swarmName = swarmId == 1 ? "ALFA" : (swarmId == 2 ? "BRAVO" : "CHARLIE");
                    targetFound = true;

                    if (laserActive) laserPainted = true;

                    string timeStr = DateTime.Now.ToString("HH:mm:ss");
                    listBox1.Items.Add($"[{timeStr}] [OVRK {swarmName}] AI Teyit: %{aiConfidence} | Lazer: {(laserActive ? "AKTIF" : "KAPALI")} | Kordinat: {targetLat}");
                    listBox1.TopIndex = listBox1.Items.Count - 1;

                    LogThreatToDatabase(deviceId, swarmId, targetLat, targetLon, aiConfidence);
                    radarPanel.Invalidate();
                }
            }
        }

        private void RadarPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int width = radarPanel.Width;
            int height = radarPanel.Height;
            int cx = width / 2;
            int cy = height / 2;

            Pen gridPen = new Pen(Color.DarkGreen, 1);
            g.DrawLine(gridPen, cx, 0, cx, height);
            g.DrawLine(gridPen, 0, cy, width, cy);
            g.DrawEllipse(gridPen, cx - 100, cy - 100, 200, 200);
            g.DrawEllipse(gridPen, cx - 200, cy - 200, 400, 400);

            g.FillEllipse(Brushes.Lime, cx - 4, cy - 4, 8, 8);
            g.DrawString("KARARGAH", new Font("Arial", 8, FontStyle.Bold), Brushes.Lime, cx + 5, cy + 5);

            if (targetFound)
            {
                float scale = 50000f;
                float x = (float)(targetLon - 32.7666) * scale + cx;
                float y = (float)(39.9666 - targetLat) * scale + cy;

                g.FillRectangle(Brushes.Red, x - 6, y - 6, 12, 12);

                if (laserPainted)
                {
                    Pen laserPen = new Pen(Color.Yellow, 2);
                    g.DrawLine(laserPen, x - 20, y, x + 20, y);
                    g.DrawLine(laserPen, x, y - 20, x, y + 20);
                    g.DrawEllipse(laserPen, x - 15, y - 15, 30, 30);
                    g.DrawString("THREAT LOCKED (LASER)", new Font("Consolas", 10, FontStyle.Bold), Brushes.Yellow, x + 15, y - 15);
                }
                else
                {
                    g.DrawString("THREAT: S-400", new Font("Consolas", 10, FontStyle.Bold), Brushes.Red, x + 15, y - 15);
                }
            }
        }
    }
}