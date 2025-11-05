using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProMag_Steam_Games
{
    public partial class SettingsForm : Form
    {
        private readonly string accessToken;
        private string? steamConfigPath;
        private string? steamDepotPath;
        private string? steamPluginPath;
        private const string ServerUrl = "http://152.53.162.136:8084";
        private Label? labelMessage;

        public SettingsForm(string token)
        {
            InitializeComponent();
            accessToken = token;
            InitializeSteamPaths();
            AddButtons();
            AddMessageLabel();
        }

        private void AddButtons()
        {
            var btnClearAll = new Button
            {
                Text = "Clear All Apps",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Location = new Point(50, 50),
                Size = new Size(200, 40)
            };
            btnClearAll.Click += async (s, e) => await ClearAllApps();
            Controls.Add(btnClearAll);

            var btnAddAll = new Button
            {
                Text = "Add All Apps",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Location = new Point(50, 100),
                Size = new Size(200, 40)
            };
            btnAddAll.Click += async (s, e) => await AddAllApps();
            Controls.Add(btnAddAll);
        }

        private void AddMessageLabel()
        {
            labelMessage = new Label
            {
                ForeColor = Color.White,
                Location = new Point(50, 150),
                Size = new Size(700, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(labelMessage);
        }

        private void InitializeSteamPaths()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
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
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
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
            }
            catch (Exception) { }
        }

        private async Task ClearAllApps()
        {
            try
            {
                if (string.IsNullOrEmpty(steamPluginPath) || string.IsNullOrEmpty(steamDepotPath))
                {
                    labelMessage!.Text = "خطأ: مسارات Steam غير محددة.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }

                var luaFiles = Directory.GetFiles(steamPluginPath, "*.lua");
                foreach (var file in luaFiles)
                {
                    File.Delete(file);
                }

                var manifestFiles = Directory.GetFiles(steamDepotPath, "*.manifest");
                foreach (var file in manifestFiles)
                {
                    File.Delete(file);
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var saveContent = new StringContent(JsonConvert.SerializeObject(new { appIds = new List<int>() }), Encoding.UTF8, "application/json");
                await client.PostAsync($"{ServerUrl}/save-games", saveContent);

                labelMessage!.Text = "تم مسح جميع الألعاب من الجهاز والسيرفر. أعد تشغيل Steam.";
                labelMessage.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                labelMessage!.Text = $"خطأ: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private async Task AddAllApps()
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
                    foreach (var appId in appIds)
                    {
                        await DownloadAndProcessApp(appId);
                    }
                    labelMessage!.Text = "تم إضافة جميع الألعاب المحفوظة إلى Steam. أعد تشغيل Steam.";
                    labelMessage.ForeColor = Color.Green;
                }
                else
                {
                    labelMessage!.Text = "فشل في جلب الألعاب من السيرفر.";
                    labelMessage.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage!.Text = $"خطأ: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }

        private async Task DownloadAndProcessApp(int appId)
        {
            string githubUrl = $"https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/{appId}";
            string zipPath = Path.Combine(Path.GetTempPath(), $"{appId}.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), $"{appId}_extract");

            if (string.IsNullOrEmpty(steamConfigPath) || !Directory.Exists(steamConfigPath))
            {
                labelMessage!.Text = "خطأ: مجلد Steam غير موجود.";
                labelMessage.ForeColor = Color.Red;
                return;
            }

            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(githubUrl);
                if (!response.IsSuccessStatusCode)
                {
                    labelMessage!.Text = $"فشل تنزيل {appId}.";
                    labelMessage.ForeColor = Color.Red;
                    return;
                }
                await File.WriteAllBytesAsync(zipPath, await response.Content.ReadAsByteArrayAsync());

                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                Directory.CreateDirectory(steamPluginPath!);
                Directory.CreateDirectory(steamDepotPath!);

                var luaFiles = Directory.GetFiles(extractPath, "*.lua", SearchOption.AllDirectories);
                foreach (var file in luaFiles)
                {
                    File.Move(file, Path.Combine(steamPluginPath!, Path.GetFileName(file)), true);
                }

                var manifestFiles = Directory.GetFiles(extractPath, "*.manifest", SearchOption.AllDirectories);
                foreach (var file in manifestFiles)
                {
                    File.Move(file, Path.Combine(steamDepotPath!, Path.GetFileName(file)), true);
                }

                Directory.Delete(extractPath, true);
                File.Delete(zipPath);
            }
            catch (Exception ex)
            {
                labelMessage!.Text = $"خطأ في {appId}: {ex.Message}";
                labelMessage.ForeColor = Color.Red;
            }
        }
    }
}