using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ProMag_Steam_Games
{
    public partial class PackagesForm : Form
    {
        private readonly List<Package> allPackages = [];
        private List<SteamApp> currentGames = [];
        private const string SteamStoreSearchUrl = "https://store.steampowered.com/api/storesearch?term={0}&cc=US&l=english";
        private const string SteamAppDetailsUrl = "https://store.steampowered.com/api/appdetails?appids={0}";
        private readonly System.Timers.Timer searchTimer;
        private readonly string accessToken;
        private string? steamConfigPath;
        private string? steamDepotPath;
        private const string ServerUrl = "http://152.53.162.136:8084";

        public PackagesForm(string token)
        {
            InitializeComponent();
            accessToken = token;
            searchTimer = new System.Timers.Timer(500) { AutoReset = false };
            searchTimer.Elapsed += SearchTimer_Elapsed;
            InitializeSteamPaths();
            flowLayoutGames.Visible = false;
            btnBack.Visible = false;
            txtSearch.Visible = false;
            labelSearch.Visible = false;
        }

        private void InitializeSteamPaths()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key != null)
                    {
                        var installPath = key.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                        {
                            steamConfigPath = Path.Combine(installPath, "config");
                            steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                            return;
                        }
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null)
                    {
                        var installPath = key.GetValue("InstallPath")?.ToString();
                        if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                        {
                            steamConfigPath = Path.Combine(installPath, "config");
                            steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                            return;
                        }
                    }
                }
                steamConfigPath = @"C:\Program Files (x86)\Steam\config";
                steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                labelMessage.Text = "تحذير: استخدام المسار الافتراضي لـ Steam.";
                labelMessage.ForeColor = Color.Yellow;
            }
            catch (Exception ex)
            {
                steamConfigPath = @"C:\Program Files (x86)\Steam\config";
                steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                labelMessage.Text = $"خطأ في مسار Steam: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private async void PackagesForm_Load(object sender, EventArgs e)
        {
            await LoadPackages();
        }

        private async Task LoadPackages()
        {
            labelLoading.Visible = true;
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync($"{ServerUrl}/get-packages");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var packageResponse = JsonConvert.DeserializeObject<PackageResponse>(json);
                    if (packageResponse == null)
                    {
                        labelMessage.Text = "فشل في معالجة استجابة السيرفر.";
                        labelMessage.ForeColor = Color.Red;
                        return;
                    }
                    allPackages.Clear();
                    allPackages.AddRange(packageResponse.Packages);
                    await UpdatePackageList(allPackages);
                }
                else
                {
                    labelMessage.Text = "فشل في تحميل الباقات من السيرفر.";
                    labelMessage.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
            finally
            {
                labelLoading.Visible = false;
            }
        }

        private async Task UpdatePackageList(List<Package> packages)
        {
            flowLayoutPackages.Controls.Clear();
            using var client = new HttpClient();
            foreach (var pkg in packages)
            {
                var panel = new Panel { Width = 300, Height = 380, BackColor = Color.FromArgb(50, 50, 50), Margin = new Padding(10) };
                var labelName = new Label { Text = pkg.Name ?? "Unknown Package", ForeColor = Color.White, Location = new Point(5, 5), Size = new Size(290, 30), TextAlign = ContentAlignment.MiddleCenter };
                panel.Controls.Add(labelName);

                int gridSize = 3;
                int imageSize = 80;
                int spacing = 5;
                for (int i = 0; i < Math.Min(9, pkg.AppIds.Count); i++)
                {
                    int appId = pkg.AppIds[i];
                    string detailsUrl = string.Format(SteamAppDetailsUrl, appId);
                    HttpResponseMessage detailsResponse = await client.GetAsync(detailsUrl);
                    if (detailsResponse.IsSuccessStatusCode)
                    {
                        var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                        var detailsObj = JObject.Parse(detailsJson);
                        var appData = detailsObj[appId.ToString()]?["data"];
                        string? headerImage = appData?["header_image"]?.ToString();

                        var pictureBox = new PictureBox
                        {
                            Size = new Size(imageSize, imageSize),
                            Location = new Point(5 + (i % gridSize) * (imageSize + spacing), 40 + (i / gridSize) * (imageSize + spacing)),
                            SizeMode = PictureBoxSizeMode.StretchImage
                        };
                        if (!string.IsNullOrEmpty(headerImage))
                        {
                            pictureBox.LoadAsync(headerImage);
                        }
                        panel.Controls.Add(pictureBox);
                    }
                }

                var buttonView = new Button
                {
                    Text = "View Games",
                    BackColor = Color.RoyalBlue,
                    ForeColor = Color.White,
                    Location = new Point(5, 340),
                    Size = new Size(290, 30),
                    Tag = pkg
                };
                buttonView.Click += async (s, e) =>
                {
                    if (s is Button btn && btn.Tag is Package selectedPkg)
                    {
                        await LoadPackageGames(selectedPkg);
                    }
                };
                panel.Controls.Add(buttonView);
                flowLayoutPackages.Controls.Add(panel);
            }
        }

        private async Task LoadPackageGames(Package pkg)
        {
            labelLoading.Visible = true;
            currentGames.Clear();
            using var client = new HttpClient();
            foreach (var appId in pkg.AppIds)
            {
                string detailsUrl = string.Format(SteamAppDetailsUrl, appId);
                HttpResponseMessage detailsResponse = await client.GetAsync(detailsUrl);
                if (detailsResponse.IsSuccessStatusCode)
                {
                    var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                    var detailsObj = JObject.Parse(detailsJson);
                    var appData = detailsObj[appId.ToString()]?["data"];
                    if (appData != null && appData["type"]?.ToString().ToLower() == "game")
                    {
                        string genres = string.Join(", ", appData["genres"]?.Select(g => g["description"]?.ToString())?.Where(g => g != null) ?? []);
                        string releaseDate = appData["release_date"]?["date"]?.ToString() ?? "Unknown";
                        string releaseYear = releaseDate.Split(',').LastOrDefault()?.Trim() ?? "Unknown";
                        currentGames.Add(new SteamApp
                        {
                            AppId = appId,
                            Name = appData["name"]?.ToString(),
                            Genres = genres,
                            ReleaseYear = releaseYear,
                            HeaderImage = appData["header_image"]?.ToString()
                        });
                    }
                }
            }
            UpdateGameList(currentGames);
            flowLayoutPackages.Visible = false;
            flowLayoutGames.Visible = true;
            btnBack.Visible = true;
            txtSearch.Visible = true;
            labelSearch.Visible = true;
            labelLoading.Visible = false;
        }

        private void UpdateGameList(List<SteamApp> games)
        {
            flowLayoutGames.Controls.Clear();
            foreach (var game in games)
            {
                var panel = new Panel { Width = 150, Height = 250, BackColor = Color.FromArgb(50, 50, 50), Margin = new Padding(10) };
                var pictureBox = new PictureBox { Size = new Size(140, 160), Location = new Point(5, 5), SizeMode = PictureBoxSizeMode.StretchImage };
                if (!string.IsNullOrEmpty(game.HeaderImage))
                {
                    pictureBox.LoadAsync(game.HeaderImage);
                }
                panel.Controls.Add(pictureBox);
                var labelName = new Label { Text = game.Name ?? "Unknown", ForeColor = Color.White, Location = new Point(5, 170), Size = new Size(140, 30), TextAlign = ContentAlignment.MiddleCenter, AutoEllipsis = true };
                panel.Controls.Add(labelName);
                var buttonAdd = new Button { Text = "Add to Steam", BackColor = Color.RoyalBlue, ForeColor = Color.White, Location = new Point(5, 205), Size = new Size(140, 30), Tag = game.AppId };
                buttonAdd.Click += async (s, e) =>
                {
                    if (s is Button btn && btn.Tag is int appId)
                    {
                        await DownloadAndProcessApp(appId);
                    }
                };
                panel.Controls.Add(buttonAdd);
                flowLayoutGames.Controls.Add(panel);
            }
        }

        private async Task DownloadAndProcessApp(int appId)
        {
            string githubUrl = $"https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/{appId}";
            string zipPath = Path.Combine(Path.GetTempPath(), $"{appId}.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), $"{appId}_extract");

            if (string.IsNullOrEmpty(steamConfigPath) || !Directory.Exists(steamConfigPath))
            {
                labelMessage.Text = $"خطأ: مجلد Steam غير موجود في {steamConfigPath ?? "غير محدد"}.";
                labelMessage.ForeColor = Color.Red;
                return;
            }

            using var client = new HttpClient();
            try
            {
                labelMessage.Text = $"جاري تنزيل {appId} من GitHub...";
                labelMessage.ForeColor = Color.Blue;
                var response = await client.GetAsync(githubUrl);
                if (!response.IsSuccessStatusCode)
                {
                    labelMessage.Text = $"فشل التنزيل (كود: {response.StatusCode}).";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }
                await File.WriteAllBytesAsync(zipPath, await response.Content.ReadAsByteArrayAsync());

                labelMessage.Text = "جاري استخراج...";
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                string steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
                Directory.CreateDirectory(steamPluginPath);
                Directory.CreateDirectory(steamDepotPath!);

                labelMessage.Text = "جاري نقل lua...";
                var luaFiles = Directory.GetFiles(extractPath, "*.lua", SearchOption.AllDirectories);
                foreach (var file in luaFiles)
                {
                    File.Move(file, Path.Combine(steamPluginPath, Path.GetFileName(file)), true);
                }

                labelMessage.Text = "جاري نقل manifest...";
                var manifestFiles = Directory.GetFiles(extractPath, "*.manifest", SearchOption.AllDirectories);
                foreach (var file in manifestFiles)
                {
                    File.Move(file, Path.Combine(steamDepotPath!, Path.GetFileName(file)), true);
                }

                labelMessage.Text = "جاري التنظيف...";
                Directory.Delete(extractPath, true);
                File.Delete(zipPath);

                await SaveAppIdToServer(appId);

                labelMessage.Text = $"تم إضافة {appId} إلى Steam. أعد تشغيل Steam.";
                labelMessage.ForeColor = Color.Green;
                await Task.Delay(5000);
                labelMessage.Text = "";
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private async Task SaveAppIdToServer(int appId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{ServerUrl}/get-saved-games");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? result = JsonConvert.DeserializeObject(json);
                    List<int> appIds = result?.appIds != null ? JsonConvert.DeserializeObject<List<int>>(result.appIds.ToString()) : [];
                    if (!appIds.Contains(appId))
                    {
                        appIds.Add(appId);
                    }
                    var saveContent = new StringContent(JsonConvert.SerializeObject(new { appIds }), Encoding.UTF8, "application/json");
                    response = await client.PostAsync($"{ServerUrl}/save-games", saveContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        labelMessage.Text = "فشل في حفظ اللعبة في السيرفر.";
                        labelMessage.ForeColor = Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ في الاتصال بالسيرفر: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private async void SearchTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (flowLayoutGames.Visible)
            {
                var filtered = currentGames.Where(g => g.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                UpdateGameList(filtered);
            }
            else
            {
                var filtered = allPackages.Where(p => p.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
                await UpdatePackageList(filtered);
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            flowLayoutGames.Visible = false;
            btnBack.Visible = false;
            flowLayoutPackages.Visible = true;
            txtSearch.Visible = false;
            labelSearch.Visible = false;
            labelMessage.Text = "";
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}