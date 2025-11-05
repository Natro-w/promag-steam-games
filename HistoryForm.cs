using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProMag_Steam_Games
{
    public partial class HistoryForm : Form
    {
        private readonly List<SteamApp> installedGames = [];
        private string? steamConfigPath;
        private string? steamDepotPath;
        private string? steamPluginPath;
        private const string SteamAppDetailsUrl = "https://store.steampowered.com/api/appdetails?appids={0}";
        private readonly string accessToken;
        private const string ServerUrl = "http://152.53.162.136:8084";

        public HistoryForm(string token)
        {
            InitializeComponent();
            accessToken = token;
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
                            steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
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
                            steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
                            return;
                        }
                    }
                }
                steamConfigPath = @"C:\Program Files (x86)\Steam\config";
                steamDepotPath = Path.Combine(steamConfigPath, "depotcache");
                steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
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
                steamPluginPath = Path.Combine(steamConfigPath, "stplug-in");
                this.Invoke(() =>
                {
                    labelMessage.Text = $"خطأ في تحديد مسار Steam: {ex.Message}. يتم استخدام المسار الافتراضي.";
                    labelMessage.ForeColor = Color.Red;
                });
            }
        }

        private async void HistoryForm_Load(object sender, EventArgs e)
        {
            await LoadInstalledGames();
        }

        private async Task LoadInstalledGames()
        {
            installedGames.Clear();
            if (string.IsNullOrEmpty(steamPluginPath) || !Directory.Exists(steamPluginPath))
            {
                labelMessage.Text = $"مجلد stplug-in غير موجود في {steamPluginPath ?? "غير محدد"}.";
                labelMessage.ForeColor = Color.Red;
                return;
            }

            try
            {
                var luaFiles = Directory.GetFiles(steamPluginPath, "*.lua", SearchOption.TopDirectoryOnly);
                foreach (var file in luaFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (int.TryParse(fileName, out int appId))
                    {
                        installedGames.Add(new SteamApp
                        {
                            AppId = appId,
                            Name = $"Game ID: {appId}",
                            HeaderImage = null
                        });
                    }
                }

                labelMessage.Text = "جاري جلب تفاصيل الألعاب من Steam...";
                labelMessage.ForeColor = Color.Blue;

                using var client = new HttpClient();
                foreach (var game in installedGames)
                {
                    try
                    {
                        string detailsUrl = string.Format(SteamAppDetailsUrl, game.AppId);
                        HttpResponseMessage detailsResponse = await client.GetAsync(detailsUrl);
                        if (detailsResponse.IsSuccessStatusCode)
                        {
                            var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                            var detailsObj = JObject.Parse(detailsJson);
                            var appData = detailsObj[game.AppId.ToString()]?["data"];
                            if (appData != null && appData["type"]?.ToString().ToLower() == "game")
                            {
                                game.Name = appData["name"]?.ToString() ?? $"Game ID: {game.AppId}";
                                game.HeaderImage = appData["header_image"]?.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        labelMessage.Text = $"خطأ في جلب تفاصيل اللعبة ID {game.AppId}: {ex.Message}. يتم استخدام قيم افتراضية.";
                        labelMessage.ForeColor = Color.Yellow;
                    }
                }

                UpdateGameList(installedGames);
                if (installedGames.Count == 0)
                {
                    labelMessage.Text = "لم يتم العثور على ألعاب مثبتة.";
                    labelMessage.ForeColor = Color.Yellow;
                }
                else
                {
                    labelMessage.Text = "تم جلب التفاصيل بنجاح!";
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
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ أثناء تحميل الألعاب: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
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
                var buttonRemove = new Button
                {
                    Text = "Remove from Steam",
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    Location = new Point(5, 205),
                    Size = new Size(140, 30),
                    Tag = game.AppId
                };
                buttonRemove.Click += async (s, e) =>
                {
                    if (s is Button btn && btn.Tag is int appId)
                    {
                        await RemoveGameFromSteam(appId);
                    }
                };
                panel.Controls.Add(buttonRemove);
                flowLayoutGames.Controls.Add(panel);
            }
        }

        private async Task RemoveGameFromSteam(int appId)
        {
            try
            {
                if (string.IsNullOrEmpty(steamPluginPath) || string.IsNullOrEmpty(steamDepotPath))
                {
                    labelMessage.Text = "خطأ: مسارات Steam غير محددة.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }

                string luaFile = Path.Combine(steamPluginPath, $"{appId}.lua");
                string manifestFile = Path.Combine(steamDepotPath, $"{appId}.manifest");

                if (File.Exists(luaFile))
                {
                    File.Delete(luaFile);
                }
                if (File.Exists(manifestFile))
                {
                    File.Delete(manifestFile);
                }

                installedGames.RemoveAll(g => g.AppId == appId);
                UpdateGameList(installedGames);
                await RemoveAppIdFromServer(appId);
                labelMessage.Text = $"تم إزالة اللعبة (ID: {appId}) من Steam. أعد تشغيل Steam.";
                labelMessage.ForeColor = Color.Green;

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
                labelMessage.Text = $"خطأ أثناء إزالة اللعبة (ID: {appId}): {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private async Task RemoveAppIdFromServer(int appId)
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
                    appIds.RemoveAll(id => id == appId);
                    var saveContent = new StringContent(JsonConvert.SerializeObject(new { appIds }), Encoding.UTF8, "application/json");
                    response = await client.PostAsync($"{ServerUrl}/save-games", saveContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        labelMessage.Text = "فشل في إزالة اللعبة من السيرفر.";
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
}