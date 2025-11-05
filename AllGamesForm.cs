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
    public partial class AllGamesForm : Form
    {
        private readonly List<SteamApp> allGames = [];
        private const string SteamStoreSearchUrl = "https://store.steampowered.com/api/storesearch?term={0}&cc=US&l=english";
        private const string SteamAppDetailsUrl = "https://store.steampowered.com/api/appdetails?appids={0}";
        private readonly System.Timers.Timer searchTimer;
        private readonly string accessToken;
        private string? steamConfigPath;
        private string? steamDepotPath;
        private const string ServerUrl = "http://152.53.162.136:8084";

        public AllGamesForm(string token)
        {
            InitializeComponent();
            accessToken = token;
            searchTimer = new System.Timers.Timer(500) { AutoReset = false };
            searchTimer.Elapsed += SearchTimer_Elapsed;
            InitializeSteamPaths();
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
                this.Invoke(() =>
                {
                    labelMessage.Text = "تحذير: لم يتم العثور على مسار Steam في السجل، يتم استخدام المسار الافتراضي.";
                    labelMessage.ForeColor = Color.Yellow;
                });
            }
            catch (Exception ex)
            {
                steamConfigPath = @"C:\Program Files (x86)\Steam\config";
                steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                this.Invoke(() =>
                {
                    labelMessage.Text = $"خطأ في تحديد مسار Steam: {ex.Message}. يتم استخدام المسار الافتراضي.";
                    labelMessage.ForeColor = Color.Red;
                });
            }
        }

        private void AllGamesForm_Load(object sender, EventArgs e)
        {
            LoadInitialGames();
        }

        private async void LoadInitialGames()
        {
            await PerformSearch("game");
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private async void SearchTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (searchText.Length is > 0 and < 3)
            {
                this.Invoke(() =>
                {
                    UpdateGameList([]);
                    labelLoading.Visible = false;
                    labelMessage.Text = "يرجى إدخال 3 أحرف على الأقل للبحث.";
                    labelMessage.ForeColor = Color.Yellow;
                });
                return;
            }
            await PerformSearch(searchText.Length > 0 ? searchText : "game");
        }

        private async Task PerformSearch(string searchText)
        {
            this.Invoke(() =>
            {
                labelLoading.Visible = true;
                labelMessage.Text = "";
            });
            try
            {
                using var client = new HttpClient();
                string searchUrl = string.Format(SteamStoreSearchUrl, Uri.EscapeDataString(searchText));
                HttpResponseMessage response = await client.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonConvert.DeserializeObject<SteamSearchResponse>(json);
                    if (searchResult?.Items is { Count: > 0 })
                    {
                        var filteredGames = new List<SteamApp>();
                        foreach (var item in searchResult.Items.Take(10))
                        {
                            string detailsUrl = string.Format(SteamAppDetailsUrl, item.Id);
                            HttpResponseMessage detailsResponse = await client.GetAsync(detailsUrl);
                            if (detailsResponse.IsSuccessStatusCode)
                            {
                                var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                                var detailsObj = JObject.Parse(detailsJson);
                                var appData = detailsObj[item.Id.ToString()]?["data"];
                                if (appData != null && appData["type"]?.ToString().ToLower() == "game")
                                {
                                    string genres = string.Join(", ", appData["genres"]?.Select(g => g["description"]?.ToString())?.Where(g => g != null) ?? []);
                                    string releaseDate = appData["release_date"]?["date"]?.ToString() ?? "Unknown";
                                    string releaseYear = releaseDate.Split(',').LastOrDefault()?.Trim() ?? "Unknown";
                                    filteredGames.Add(new SteamApp
                                    {
                                        AppId = item.Id,
                                        Name = appData["name"]?.ToString(),
                                        Genres = genres,
                                        ReleaseYear = releaseYear,
                                        HeaderImage = appData["header_image"]?.ToString()
                                    });
                                }
                            }
                        }
                        allGames.Clear();
                        allGames.AddRange(filteredGames);
                        this.Invoke(() =>
                        {
                            UpdateGameList(allGames);
                            labelLoading.Visible = false;
                            labelMessage.Text = "تم البحث بنجاح!";
                            labelMessage.ForeColor = Color.Green;
                            var timer = new System.Timers.Timer(5000);
                            timer.Elapsed += (s, ev) =>
                            {
                                this.Invoke(() =>
                                {
                                    labelMessage.Text = "";
                                });
                                timer.Stop();
                            };
                            timer.Start();
                        });
                    }
                    else
                    {
                        this.Invoke(() =>
                        {
                            UpdateGameList([]);
                            labelLoading.Visible = false;
                            labelMessage.Text = "لم يتم العثور على نتائج.";
                            labelMessage.ForeColor = Color.Yellow;
                        });
                    }
                }
                else
                {
                    this.Invoke(() =>
                    {
                        labelLoading.Visible = false;
                        labelMessage.Text = "فشل في البحث عن الألعاب.";
                        labelMessage.ForeColor = Color.Red;
                    });
                }
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    labelLoading.Visible = false;
                    labelMessage.Text = $"خطأ: {ex.Message}";
                    labelMessage.ForeColor = Color.Red;
                });
            }
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
                var buttonAdd = new Button
                {
                    Text = "Add to Steam",
                    BackColor = Color.RoyalBlue,
                    ForeColor = Color.White,
                    Location = new Point(5, 205),
                    Size = new Size(140, 30),
                    Tag = game.AppId
                };
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

            if (!Directory.Exists(steamConfigPath))
            {
                this.Invoke(() =>
                {
                    labelMessage.Text = $"خطأ: مجلد إعدادات Steam غير موجود في: {steamConfigPath}. تحقق من تثبيت Steam.";
                    labelMessage.ForeColor = Color.Red;
                });
                return;
            }

            using var client = new HttpClient();
            try
            {
                this.Invoke(() =>
                {
                    labelMessage.Text = $"جاري تنزيل اللعبة (ID: {appId}) من GitHub...";
                    labelMessage.ForeColor = Color.Blue;
                });
                var response = await client.GetAsync(githubUrl);
                if (!response.IsSuccessStatusCode)
                {
                    this.Invoke(() =>
                    {
                        labelMessage.Text = $"فشل تنزيل اللعبة (ID: {appId}) من GitHub (كود: {response.StatusCode}). اللعبة غير متوفرة.";
                        labelMessage.ForeColor = Color.Red;
                    });
                    return;
                }
                this.Invoke(() =>
                {
                    labelMessage.Text = "تم التنزيل بنجاح، جاري حفظ ملف zip...";
                    labelMessage.ForeColor = Color.Blue;
                });
                await File.WriteAllBytesAsync(zipPath, await response.Content.ReadAsByteArrayAsync());

                this.Invoke(() =>
                {
                    labelMessage.Text = "جاري استخراج الملفات من zip...";
                    labelMessage.ForeColor = Color.Blue;
                });
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                string steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
                Directory.CreateDirectory(steamPluginPath);
                if (!string.IsNullOrEmpty(steamDepotPath))
                {
                    Directory.CreateDirectory(steamDepotPath);
                }
                else
                {
                    this.Invoke(() =>
                    {
                        labelMessage.Text = "خطأ: مسار depotcache غير محدد.";
                        labelMessage.ForeColor = Color.Red;
                    });
                    return;
                }

                this.Invoke(() =>
                {
                    labelMessage.Text = "جاري نقل ملفات .lua إلى Steam...";
                    labelMessage.ForeColor = Color.Blue;
                });
                var luaFiles = Directory.GetFiles(extractPath, "*.lua", SearchOption.AllDirectories);
                foreach (var file in luaFiles)
                {
                    string dest = Path.Combine(steamPluginPath, Path.GetFileName(file));
                    File.Move(file, dest, true);
                }

                this.Invoke(() =>
                {
                    labelMessage.Text = "جاري نقل ملفات .manifest إلى Steam...";
                    labelMessage.ForeColor = Color.Blue;
                });
                var manifestFiles = Directory.GetFiles(extractPath, "*.manifest", SearchOption.AllDirectories);
                foreach (var file in manifestFiles)
                {
                    string dest = Path.Combine(steamDepotPath, Path.GetFileName(file));
                    File.Move(file, dest, true);
                }

                this.Invoke(() =>
                {
                    labelMessage.Text = "جاري تنظيف الملفات المؤقتة...";
                    labelMessage.ForeColor = Color.Blue;
                });
                Directory.Delete(extractPath, true);
                File.Delete(zipPath);

                await SaveAppIdToServer(appId);

                this.Invoke(() =>
                {
                    labelMessage.Text = $"تمت إضافة اللعبة (ID: {appId}) إلى Steam بنجاح في {steamConfigPath}. أعد تشغيل Steam.";
                    labelMessage.ForeColor = Color.Green;
                });

                var timer = new System.Timers.Timer(5000);
                timer.Elapsed += (s, e) =>
                {
                    this.Invoke(() =>
                    {
                        labelMessage.Text = "";
                    });
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    labelMessage.Text = $"خطأ أثناء معالجة اللعبة (ID: {appId}): {ex.Message}";
                    labelMessage.ForeColor = Color.Red;
                });
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

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class SteamSearchResponse
    {
        public int Total { get; set; }
        public List<SteamSearchItem>? Items { get; set; }
    }

    public class SteamSearchItem
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public int Id { get; set; }
        public SteamPrice? Price { get; set; }
        public string? TinyImage { get; set; }
        public string? Metascore { get; set; }
        public SteamPlatforms? Platforms { get; set; }
        public bool StreamingVideo { get; set; }
        public string? ControllerSupport { get; set; }
    }

    public class SteamPrice
    {
        public string? Currency { get; set; }
        public int Initial { get; set; }
        public int Final { get; set; }
    }

    public class SteamPlatforms
    {
        public bool Windows { get; set; }
        public bool Mac { get; set; }
        public bool Linux { get; set; }
    }
}