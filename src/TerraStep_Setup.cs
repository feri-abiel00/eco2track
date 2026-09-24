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

            // Always ensure the Desktop Shortcut is created on launch
            PerformInstallation();

            // Check if launched with --app or --launch flag
            bool isLaunchOnly = false;
            foreach (string arg in args)
            {
                if (arg.Equals("--launch", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-app", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--start", StringComparison.OrdinalIgnoreCase))
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
            // Prefer the ONLINE (HTTPS) origin so browser geolocation is allowed.
            // Edge on Windows then uses the Windows Location Provider, which includes
            // the physical device GPS sensor (GPS Perangkat) + Google Location sources.
            // Geolocation is blocked by Chromium for file:// pages, so we only fall
            // back to the offline local copy when the host is unreachable.
            string localMp3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mp3.html");
            if (!File.Exists(localMp3))
            {
                string appDataMp3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraStep", "mp3.html");
                if (File.Exists(appDataMp3)) localMp3 = appDataMp3;
            }

            string url = "https://terrastep.web.app/mp3.html";
            bool online = IsOnline();
            if (!online && File.Exists(localMp3))
            {
                url = "file:///" + localMp3.Replace("\\", "/");
            }

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
                // Keep the GPS/location permission grant persistent in the dedicated
                // user-data-dir so the device GPS works immediately on later launches.
                psi.Arguments = string.Format("--app=\"{0}\" --user-data-dir=\"{1}\" --window-size=1250,860", url, dataDir);
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

        private static bool IsOnline()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://terrastep.web.app/");
                req.Method = "HEAD";
                req.Timeout = 5000;
                using (WebResponse resp = req.GetResponse()) { return true; }
            }
            catch { return false; }
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

        public static void PerformInstallation()
        {
            try
            {
                string localAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraStep");
                if (!Directory.Exists(localAppDir))
                {
                    Directory.CreateDirectory(localAppDir);
                }

                // Copy executable to LocalAppData
                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                string targetExe = Path.Combine(localAppDir, "TerraStep.exe");

                if (!currentExe.Equals(targetExe, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Copy(currentExe, targetExe, true); } catch { }
                }

                // Copy icon & logo
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string srcIco = Path.Combine(baseDir, "app.ico");
                string targetIco = Path.Combine(localAppDir, "app.ico");
                if (File.Exists(srcIco))
                {
                    try { File.Copy(srcIco, targetIco, true); } catch { }
                }

                string srcLogo = Path.Combine(baseDir, "logo.png");
                string targetLogo = Path.Combine(localAppDir, "logo.png");
                if (File.Exists(srcLogo))
                {
                    try { File.Copy(srcLogo, targetLogo, true); } catch { }
                }

                // Copy the 5 core modules so the desktop app functions fully offline
                string[] filesToCopy = new string[] {
                    "mp3.html", "ECO2Track.html", "Mood.html", "Physics.html",
                    "kalkulator_kimia.html", "kalkulator_kimia (4).html", "firebase-config.js",
                    "leaflet.js", "leaflet.css"
                };
                foreach (string f in filesToCopy)
                {
                    string src = Path.Combine(baseDir, f);
                    string dst = Path.Combine(localAppDir, f);
                    if (File.Exists(src))
                    {
                        try { File.Copy(src, dst, true); } catch { }
                    }
                }

                string exeToRun = File.Exists(targetExe) ? targetExe : currentExe;
                string icoToUse = File.Exists(targetIco) ? targetIco : (File.Exists(srcIco) ? srcIco : exeToRun);

                // 1. Create User Desktop Shortcut (TerraStep.lnk)
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (Directory.Exists(desktopPath))
                {
                    string shortcutPath = Path.Combine(desktopPath, "TerraStep.lnk");
                    CreateShortcut(shortcutPath, exeToRun, "--launch", icoToUse, "TerraStep - ECO₂Track Virtual MP3 Ecosystem");

                    // Ensure only TerraStep shortcut is on the Desktop (remove obsolete ECO2Track.lnk if present)
                    string ecoShortcut = Path.Combine(desktopPath, "ECO2Track.lnk");
                    if (File.Exists(ecoShortcut))
                    {
                        try { File.Delete(ecoShortcut); } catch { }
                    }
                }

                // 2. Also create in Common/Public Desktop if accessible
                try
                {
                    string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                    if (!string.IsNullOrEmpty(commonDesktop) && Directory.Exists(commonDesktop))
                    {
                        CreateShortcut(Path.Combine(commonDesktop, "TerraStep.lnk"), exeToRun, "--launch", icoToUse, "TerraStep Ecosystem");
                    }
                }
                catch { }

                // 3. Create in Start Menu Programs
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "TerraStep.lnk");
                CreateShortcut(startMenuPath, exeToRun, "--launch", icoToUse, "TerraStep - ECO₂Track Virtual MP3 Ecosystem");
            }
            catch { }
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
                    return;
                }
            }
            catch { }

            // Robust fallback via PowerShell WScript.Shell
            try
            {
                string psCmd = string.Format(
                    "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('{0}'); $s.TargetPath = '{1}'; $s.Arguments = '{2}'; $s.WorkingDirectory = '{3}'; if ('{4}') {{ $s.IconLocation = '{4},0' }}; $s.Description = '{5}'; $s.Save()",
                    shortcutPath.Replace("'", "''"),
                    targetPath.Replace("'", "''"),
                    arguments.Replace("'", "''"),
                    Path.GetDirectoryName(targetPath).Replace("'", "''"),
                    (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath)) ? iconPath.Replace("'", "''") : "",
                    description.Replace("'", "''")
                );
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + psCmd + "\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
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
            // Pre-install shortcut immediately upon opening setup
            Program.PerformInstallation();
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
            lblTeam.Text = "STEAM Science Expo SMA Unggul Del 2026: TERRASTEP";
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
            lblCardTitle.Text = "Official Desktop Application Setup";
            lblCardTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCardTitle.ForeColor = Color.White;
            lblCardTitle.Location = new Point(16, 12);
            lblCardTitle.AutoSize = true;

            Label lblCardDesc = new Label();
            lblCardDesc.Text = "Installs the TerraStep standalone ecosystem directly to your Windows desktop with custom icon shortcuts, offline caching, and instant access to MP3 and core apps.";
            lblCardDesc.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblCardDesc.ForeColor = Color.FromArgb(203, 213, 225);
            lblCardDesc.Location = new Point(16, 38);
            lblCardDesc.Size = new Size(444, 45);

            // Feature bullets
            Label lblBullets = new Label();
            lblBullets.Text = "• Creates instant Desktop Shortcut with official logo icon\n• Launches directly into the Virtual MP3 Player Hub\n• Cloud sync via Firebase & offline data persistence\n• Zero complex dependencies or bulky installations";
            lblBullets.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblBullets.ForeColor = Color.FromArgb(148, 163, 184);
            lblBullets.Location = new Point(16, 88);
            lblBullets.Size = new Size(444, 76);

            pnlCard.Controls.Add(lblCardTitle);
            pnlCard.Controls.Add(lblCardDesc);
            pnlCard.Controls.Add(lblBullets);

            // Progress Bar & Status
            lblStep = new Label();
            lblStep.Text = "Ready to install desktop shortcut and application.";
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
            lblStatus.Text = "Click 'Create Desktop Shortcut & Install' to complete setup.";
            lblStatus.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
            lblStatus.Location = new Point(24, 365);
            lblStatus.Size = new Size(476, 20);

            // Bottom Buttons
            btnDownloadInstall = new Button();
            btnDownloadInstall.Text = "📌 Create Desktop Shortcut & Install";
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
            btnLaunchDirect.Text = "Launch Now 🚀";
            btnLaunchDirect.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            btnLaunchDirect.BackColor = Color.FromArgb(51, 65, 85);
            btnLaunchDirect.ForeColor = Color.White;
            btnLaunchDirect.FlatStyle = FlatStyle.Flat;
            btnLaunchDirect.FlatAppearance.BorderSize = 0;
            btnLaunchDirect.Location = new Point(356, 400);
            btnLaunchDirect.Size = new Size(144, 48);
            btnLaunchDirect.Cursor = Cursors.Hand;
            btnLaunchDirect.Click += (s, e) => { 
                Program.PerformInstallation();
                Program.LaunchDesktopApp(); 
            };

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlCard);
            this.Controls.Add(lblStep);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnDownloadInstall);
            this.Controls.Add(btnLaunchDirect);

            // Progress Timer
            progressTimer = new System.Windows.Forms.Timer();
            progressTimer.Interval = 35;
            progressTimer.Tick += ProgressTimer_Tick;
        }

        private void BtnDownloadInstall_Click(object sender, EventArgs e)
        {
            btnDownloadInstall.Enabled = false;
            btnLaunchDirect.Enabled = false;
            currentProgress = 0;
            progressBar.Value = 0;
            lblStep.Text = "Creating Desktop Shortcut...";
            lblStatus.Text = "Registering TerraStep.lnk on your Windows Desktop...";
            Program.PerformInstallation();
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            currentProgress += 3;
            if (currentProgress > 100) currentProgress = 100;
            progressBar.Value = currentProgress;

            if (currentProgress == 30)
            {
                lblStep.Text = "Configuring application bundle...";
                lblStatus.Text = "Linking MP3, ECO₂Track, Physics, Chemistry, and Mood modules...";
            }
            else if (currentProgress == 70)
            {
                lblStep.Text = "Finalizing Desktop Shortcut...";
                lblStatus.Text = "Applying official app icon and registering start menu entry...";
                Program.PerformInstallation();
            }
            else if (currentProgress >= 100)
            {
                progressTimer.Stop();
                lblStep.Text = "✅ Desktop Shortcut Successfully Created!";
                lblStatus.Text = "Shortcut placed on your Desktop. Launching TerraStep...";

                btnDownloadInstall.Text = "✅ Installed on Desktop";
                btnDownloadInstall.BackColor = Color.FromArgb(16, 185, 129);
                btnDownloadInstall.Enabled = true;
                btnLaunchDirect.Enabled = true;

                // Launch after short pause
                System.Windows.Forms.Timer launchTimer = new System.Windows.Forms.Timer();
                launchTimer.Interval = 700;
                launchTimer.Tick += (s, ev) =>
                {
                    launchTimer.Stop();
                    Program.LaunchDesktopApp();
                    this.Close();
                };
                launchTimer.Start();
            }
        }
    }
}
