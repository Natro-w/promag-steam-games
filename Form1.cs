using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
namespace ProMag_Steam_Games
{
    public partial class Form1 : Form
    {
        private bool isLoggedIn = false;
        private string? accessToken = null;
        private string? currentUsername = null;
        private string? currentRefreshToken = null;
        private const string ServerUrl = "http://152.53.162.136:8084";
        public Form1()
        {
            InitializeComponent();
            btnAllGames.MouseEnter += (s, e) => btnAllGames.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnAllGames.MouseLeave += (s, e) => btnAllGames.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnPackages.MouseEnter += (s, e) => btnPackages.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnPackages.MouseLeave += (s, e) => btnPackages.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnHistory.MouseEnter += (s, e) => btnHistory.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnHistory.MouseLeave += (s, e) => btnHistory.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnSettings.MouseEnter += (s, e) => btnSettings.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnSettings.MouseLeave += (s, e) => btnSettings.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = System.Drawing.Color.FromArgb(0, 100, 180);
            btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            string? encryptedRefreshToken = Properties.Settings.Default.RefreshToken;
            if (!string.IsNullOrEmpty(encryptedRefreshToken))
            {
                string? savedRefreshToken = DecryptString(encryptedRefreshToken);
                if (!string.IsNullOrEmpty(savedRefreshToken))
                {
                    await RefreshAccessToken(savedRefreshToken);
                }
            }
        }
        private static string EncryptString(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] protectedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }
        private static string? DecryptString(string input)
        {
            try
            {
                byte[] data = Convert.FromBase64String(input);
                byte[] unprotectedData = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(unprotectedData);
            }
            catch
            {
                return null;
            }
        }
        private async Task RefreshAccessToken(string refreshToken)
        {
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(new { refreshToken }), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{ServerUrl}/refresh", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? result = JsonConvert.DeserializeObject(json);
                    if (result == null)
                    {
                        labelMessage.Text = "فشل في معالجة استجابة السيرفر.";
                        labelMessage.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                    accessToken = result.token?.ToString();
                    currentUsername = result.username?.ToString();
                    currentRefreshToken = refreshToken;
                    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(currentUsername))
                    {
                        labelMessage.Text = "بيانات الاستجابة غير مكتملة.";
                        labelMessage.ForeColor = System.Drawing.Color.Red;
                        Properties.Settings.Default.RefreshToken = null;
                        Properties.Settings.Default.Save();
                        return;
                    }
                    isLoggedIn = true;
                    labelStatus.Text = "Status: Logged in";
                    labelStatus.ForeColor = System.Drawing.Color.Green;
                    labelWelcome.Text = $"Welcome, {currentUsername}";
                    labelWelcome.Visible = true;
                    panelLogin.Visible = false;
                    labelMessage.Text = "تم تسجيل الدخول تلقائيًا!";
                    labelMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    Properties.Settings.Default.RefreshToken = null;
                    Properties.Settings.Default.Save();
                    labelMessage.Text = "فشل في تجديد الجلسة. يرجى تسجيل الدخول يدويًا.";
                    labelMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ في الاتصال: {ex.Message}";
                labelMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                labelMessage.Text = "يرجى إدخال اسم المستخدم وكلمة المرور!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            using var client = new HttpClient();
            var loginData = new { username = txtUsername.Text, password = txtPassword.Text };
            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{ServerUrl}/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? result = JsonConvert.DeserializeObject(json);
                    if (result == null)
                    {
                        labelMessage.Text = "فشل في معالجة استجابة السيرفر.";
                        labelMessage.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                    accessToken = result.token?.ToString();
                    string? refreshToken = result.refreshToken?.ToString();
                    if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    {
                        labelMessage.Text = "بيانات الاستجابة غير مكتملة.";
                        labelMessage.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                    currentUsername = txtUsername.Text;
                    currentRefreshToken = refreshToken;
                    isLoggedIn = true;
                    labelStatus.Text = "Status: Logged in";
                    labelStatus.ForeColor = System.Drawing.Color.Green;
                    labelWelcome.Text = $"Welcome, {currentUsername}";
                    labelWelcome.Visible = true;
                    panelLogin.Visible = false;
                    labelMessage.Text = "تم تسجيل الدخول بنجاح!";
                    labelMessage.ForeColor = System.Drawing.Color.Green;
                    if (chkRememberMe.Checked)
                    {
                        Properties.Settings.Default.RefreshToken = EncryptString(refreshToken);
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    dynamic? error = JsonConvert.DeserializeObject(errorJson);
                    labelMessage.Text = error?.error?.ToString() ?? "خطأ غير معروف في تسجيل الدخول.";
                    labelMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ في الاتصال بالسيرفر: {ex.Message}";
                labelMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                labelMessage.Text = "يرجى إدخال اسم المستخدم وكلمة المرور!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            using var client = new HttpClient();
            var registerData = new { username = txtUsername.Text, password = txtPassword.Text };
            var content = new StringContent(JsonConvert.SerializeObject(registerData), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{ServerUrl}/register", content);
                if (response.IsSuccessStatusCode)
                {
                    var loginData = new { username = txtUsername.Text, password = txtPassword.Text };
                    var loginContent = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
                    response = await client.PostAsync($"{ServerUrl}/login", loginContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic? result = JsonConvert.DeserializeObject(json);
                        if (result == null)
                        {
                            labelMessage.Text = "فشل في معالجة استجابة السيرفر.";
                            labelMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }
                        accessToken = result.token?.ToString();
                        string? refreshToken = result.refreshToken?.ToString();
                        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                        {
                            labelMessage.Text = "بيانات الاستجابة غير مكتملة.";
                            labelMessage.ForeColor = System.Drawing.Color.Red;
                            return;
                        }
                        currentUsername = txtUsername.Text;
                        currentRefreshToken = refreshToken;
                        isLoggedIn = true;
                        labelStatus.Text = "Status: Logged in";
                        labelStatus.ForeColor = System.Drawing.Color.Green;
                        labelWelcome.Text = $"Welcome, {currentUsername}";
                        labelWelcome.Visible = true;
                        panelLogin.Visible = false;
                        labelMessage.Text = "تم التسجيل وتسجيل الدخول بنجاح!";
                        labelMessage.ForeColor = System.Drawing.Color.Green;
                        if (chkRememberMe.Checked)
                        {
                            Properties.Settings.Default.RefreshToken = EncryptString(refreshToken);
                            Properties.Settings.Default.Save();
                        }
                    }
                    else
                    {
                        labelMessage.Text = "تم التسجيل بنجاح، لكن فشل تسجيل الدخول التلقائي. حاول تسجيل الدخول يدويًا.";
                        labelMessage.ForeColor = System.Drawing.Color.Yellow;
                    }
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    dynamic? error = JsonConvert.DeserializeObject(errorJson);
                    labelMessage.Text = error?.error?.ToString() ?? "خطأ غير معروف في التسجيل.";
                    labelMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ في الاتصال بالسيرفر: {ex.Message}";
                labelMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            if (!isLoggedIn || string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(currentRefreshToken))
            {
                labelMessage.Text = "أنت غير مسجل الدخول!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var content = new StringContent(JsonConvert.SerializeObject(new { refreshToken = currentRefreshToken }), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{ServerUrl}/logout", content);
                if (response.IsSuccessStatusCode)
                {
                    Properties.Settings.Default.RefreshToken = null;
                    Properties.Settings.Default.Save();
                    isLoggedIn = false;
                    accessToken = null;
                    currentUsername = null;
                    currentRefreshToken = null;
                    labelWelcome.Visible = false;
                    panelLogin.Visible = true;
                    labelStatus.Text = "Status: Not logged in";
                    labelStatus.ForeColor = System.Drawing.Color.White;
                    labelMessage.Text = "تم تسجيل الخروج بنجاح!";
                    labelMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    labelMessage.Text = "فشل في تسجيل الخروج. حاول مرة أخرى.";
                    labelMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                labelMessage.Text = $"خطأ في الاتصال: {ex.Message}";
                labelMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
        private void BtnAllGames_Click(object sender, EventArgs e)
        {
            /*if (!isLoggedIn || accessToken == null)
            {
                labelMessage.Text = "يرجى تسجيل الدخول أولاً!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }*/
            AllGamesForm allGamesForm = new AllGamesForm(accessToken);
            allGamesForm.ShowDialog();
        }
        private void BtnPackages_Click(object sender, EventArgs e)
        {
            /*if (!isLoggedIn || accessToken == null)
            {
                labelMessage.Text = "يرجى تسجيل الدخول أولاً!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }*/
            PackagesForm packagesForm = new PackagesForm(accessToken);
            packagesForm.ShowDialog();
        }
        private void BtnHistory_Click(object sender, EventArgs e)
        {
            /*if (!isLoggedIn || accessToken == null)
            {
                labelMessage.Text = "يرجى تسجيل الدخول أولاً!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }*/
            HistoryForm historyForm = new HistoryForm(accessToken);
            historyForm.ShowDialog();
        }
        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (!isLoggedIn || accessToken == null)
            {
                labelMessage.Text = "يرجى تسجيل الدخول أولاً!";
                labelMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            SettingsForm settingsForm = new SettingsForm(accessToken);
            settingsForm.ShowDialog();
        }
    }
}