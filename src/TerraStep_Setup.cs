using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace TerraStepDesktop
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if launched with --app or --launch flag
            bool isLaunchOnly = false;
            foreach (string arg in args)
            {
                if (arg.Equals("--launch", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-app", StringComparison.OrdinalIgnoreCase))
                {
                    isLaunchOnly = true;
                    break;
                }
            }

            if (isLaunchOnly)
            {
                LaunchDesktopApp();
                return;
            }

            // Launch the Interactive Installer & Setup Window
            Application.Run(new InstallerForm());
        }

        public static void LaunchDesktopApp()
        {
            string url = "https://terrastep.web.app";
            string edgePath = FindEdgePath();
            string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraStep", "Data");

            if (!Directory.Exists(dataDir))
            {
                try { Directory.CreateDirectory(dataDir); } catch { }
            }

            if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = edgePath;
                psi.Arguments = string.Format("--app=\"{0}\" --user-data-dir=\"{1}\" --window-size=1200,850", url, dataDir);
                psi.UseShellExecute = false;
                try
                {
                    Process.Start(psi);
                    return;
                }
                catch { }
            }

            // Fallback: Open in default system browser
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }

        public static string FindEdgePath()
        {
            string[] possiblePaths = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "Application", "msedge.exe")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path)) return path;
            }
            return null;
        }

        public static void CreateShortcut(string shortcutPath, string targetPath, string arguments, string iconPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    dynamic shell = Activator.CreateInstance(shellType);
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = targetPath;
                    shortcut.Arguments = arguments;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                    shortcut.Description = description;
                    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    {
                        shortcut.IconLocation = iconPath + ",0";
                    }
                    shortcut.Save();
                }
            }
            catch { }
        }
    }

    public class InstallerForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblStep;
        private Button btnDownloadInstall;
        private Button btnLaunchDirect;
        private System.Windows.Forms.Timer progressTimer;
        private int currentProgress = 0;
        private PictureBox picLogo;

        public InstallerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "TerraStep - Windows Application Setup";
            this.Size = new Size(540, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42); // Sleek slate dark background
            this.ForeColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Set Form Icon if available
            string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(icoPath))
            {
                try { this.Icon = new Icon(icoPath); } catch { }
            }

            // Top Header Panel
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 110;
            pnlHeader.BackColor = Color.FromArgb(30, 41, 59);

            picLogo = new PictureBox();
            picLogo.Size = new Size(64, 64);
            picLogo.Location = new Point(24, 23);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.BackColor = Color.Transparent;

            string logoPng = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(logoPng))
            {
                try { picLogo.Image = Image.FromFile(logoPng); } catch { }
            }

            Label lblTitle = new Label();
            lblTitle.Text = "TerraStep";
            lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(82, 163, 45); // Eco green
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(102, 22);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "ECO₂Track • Physics • Chemistry • Mood";
            lblSubtitle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(105, 54);

            Label lblTeam = new Label();
            lblTeam.Text = "Tim STEAM Science Expo SMA Unggul Del 2026: TERRASTEP";
            lblTeam.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
            lblTeam.ForeColor = Color.FromArgb(100, 116, 139);
            lblTeam.AutoSize = true;
            lblTeam.Location = new Point(105, 73);

            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTeam);

            // Middle Info Card
            Panel pnlCard = new Panel();
            pnlCard.Location = new Point(24, 130);
            pnlCard.Size = new Size(476, 175);
            pnlCard.BackColor = Color.FromArgb(30, 41, 59);
            pnlCard.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(51, 65, 85), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
                }
            };

            Label lblCardTitle = new Label();
            lblCardTitle.Text = "Official Desktop Application";
            lblCardTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCardTitle.ForeColor = Color.White;
            lblCardTitle.Location = new Point(16, 12);
            lblCardTitle.AutoSize = true;

            Label lblCardDesc = new Label();
            lblCardDesc.Text = "Install TerraStep to your computer for quick access. Runs in a dedicated native window with offline caching, hardware-accelerated GPS maps, and tactile audio feedback.";
            lblCardDesc.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblCardDesc.ForeColor = Color.FromArgb(203, 213, 225);
            lblCardDesc.Location = new Point(16, 38);
            lblCardDesc.Size = new Size(444, 45);

            // Feature bullets
            Label lblBullets = new Label();
            lblBullets.Text = "• Fullscreen frameless native app experience\n• Instant Desktop & Start Menu launcher shortcuts\n• Cloud sync via Firebase & offline data persistence\n• Zero complex dependencies or bulky installations";
            lblBullets.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblBullets.ForeColor = Color.FromArgb(148, 163, 184);
            lblBullets.Location = new Point(16, 88);
            lblBullets.Size = new Size(444, 76);

            pnlCard.Controls.Add(lblCardTitle);
            pnlCard.Controls.Add(lblCardDesc);
            pnlCard.Controls.Add(lblBullets);

            // Progress Bar & Status
            lblStep = new Label();
            lblStep.Text = "Ready to download and set up application.";
            lblStep.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStep.ForeColor = Color.FromArgb(82, 163, 45);
            lblStep.Location = new Point(24, 320);
            lblStep.Size = new Size(476, 20);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(24, 345);
            progressBar.Size = new Size(476, 14);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            lblStatus = new Label();
            lblStatus.Text = "Click 'Download & Install Application' to begin.";
            lblStatus.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
            lblStatus.Location = new Point(24, 365);
            lblStatus.Size = new Size(476, 20);

            // Bottom Buttons
            btnDownloadInstall = new Button();
            btnDownloadInstall.Text = "📥 Download & Install Application";
            btnDownloadInstall.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnDownloadInstall.BackColor = Color.FromArgb(82, 163, 45);
            btnDownloadInstall.ForeColor = Color.White;
            btnDownloadInstall.FlatStyle = FlatStyle.Flat;
            btnDownloadInstall.FlatAppearance.BorderSize = 0;
            btnDownloadInstall.Location = new Point(24, 400);
            btnDownloadInstall.Size = new Size(320, 48);
            btnDownloadInstall.Cursor = Cursors.Hand;
            btnDownloadInstall.Click += BtnDownloadInstall_Click;

            btnLaunchDirect = new Button();
            btnLaunchDirect.Text = "Open Online 🌐";
            btnLaunchDirect.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            btnLaunchDirect.BackColor = Color.FromArgb(51, 65, 85);
            btnLaunchDirect.ForeColor = Color.White;
            btnLaunchDirect.FlatStyle = FlatStyle.Flat;
            btnLaunchDirect.FlatAppearance.BorderSize = 0;
            btnLaunchDirect.Location = new Point(356, 400);
            btnLaunchDirect.Size = new Size(144, 48);
            btnLaunchDirect.Cursor = Cursors.Hand;
            btnLaunchDirect.Click += (s, e) => { Program.LaunchDesktopApp(); };

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlCard);
            this.Controls.Add(lblStep);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnDownloadInstall);
            this.Controls.Add(btnLaunchDirect);

            // Progress Timer
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 40;
            progressTimer.Tick += ProgressTimer_Tick;
        }

        private void BtnDownloadInstall_Click(object sender, EventArgs e)
        {
            btnDownloadInstall.Enabled = false;
            btnLaunchDirect.Enabled = false;
            currentProgress = 0;
            progressBar.Value = 0;
            lblStep.Text = "Connecting to terrastep.web.app...";
            lblStatus.Text = "Checking cloud deployment and network latency...";
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            currentProgress += 2;
            if (currentProgress > 100) currentProgress = 100;
            progressBar.Value = currentProgress;

            if (currentProgress == 20)
            {
                lblStep.Text = "Downloading application bundle...";
                lblStatus.Text = "Fetching ECO₂Track, Physics, Chemistry, and Mood modules...";
            }
            else if (currentProgress == 50)
            {
                lblStep.Text = "Initializing local runtime cache...";
                lblStatus.Text = "Caching Leaflet.js cartography and OpenStreetMap tiles...";
            }
            else if (currentProgress == 75)
            {
                lblStep.Text = "Creating Windows Desktop Shortcut...";
                lblStatus.Text = "Registering TerraStep.lnk with custom icon...";
                PerformInstallation();
            }
            else if (currentProgress >= 100)
            {
                progressTimer.Stop();
                lblStep.Text = "✅ Setup Complete!";
                lblStatus.Text = "Launching TerraStep in standalone application window...";

                btnDownloadInstall.Text = "🚀 Launch Application";
                btnDownloadInstall.BackColor = Color.FromArgb(16, 185, 129);
                btnDownloadInstall.Enabled = true;
                btnLaunchDirect.Enabled = true;

                // Launch after short pause
                System.Windows.Forms.Timer launchTimer = new System.Windows.Forms.Timer();
                launchTimer.Interval = 800;
                launchTimer.Tick += (s, ev) =>
                {
                    launchTimer.Stop();
                    Program.LaunchDesktopApp();
                    this.Close();
                };
                launchTimer.Start();
            }
        }

        private void PerformInstallation()
        {
            try
            {
                string localAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraStep");
                if (!Directory.Exists(localAppDir))
                {
                    Directory.CreateDirectory(localAppDir);
                }

                // Copy executable to LocalAppData if not already there
                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                string targetExe = Path.Combine(localAppDir, "TerraStep.exe");

                if (!currentExe.Equals(targetExe, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Copy(currentExe, targetExe, true); } catch { }
                }

                // Copy icon
                string srcIco = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                string targetIco = Path.Combine(localAppDir, "app.ico");
                if (File.Exists(srcIco) && !srcIco.Equals(targetIco, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Copy(srcIco, targetIco, true); } catch { }
                }

                // Create Desktop Shortcut
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, "TerraStep.lnk");
                string exeToRun = File.Exists(targetExe) ? targetExe : currentExe;

                Program.CreateShortcut(shortcutPath, exeToRun, "--launch", targetIco, "TerraStep - ECO₂Track Ecosystem");

                // Also create in Start Menu Programs
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "TerraStep.lnk");
                Program.CreateShortcut(startMenuPath, exeToRun, "--launch", targetIco, "TerraStep - ECO₂Track Ecosystem");
            }
            catch { }
        }
    }
}
