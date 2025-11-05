namespace ProMag_Steam_Games
{
    partial class PackagesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.flowLayoutPackages = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutGames = new System.Windows.Forms.FlowLayoutPanel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.labelSearch = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.labelLoading = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.flowLayoutPackages.AutoScroll = true;
            this.flowLayoutPackages.Location = new System.Drawing.Point(12, 50);
            this.flowLayoutPackages.Name = "flowLayoutPackages";
            this.flowLayoutPackages.Size = new System.Drawing.Size(776, 350);
            this.flowLayoutPackages.TabIndex = 0;
            this.flowLayoutPackages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));

            this.flowLayoutGames.AutoScroll = true;
            this.flowLayoutGames.Location = new System.Drawing.Point(12, 50);
            this.flowLayoutGames.Name = "flowLayoutGames";
            this.flowLayoutGames.Size = new System.Drawing.Size(776, 350);
            this.flowLayoutGames.TabIndex = 1;
            this.flowLayoutGames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));

            this.txtSearch.Location = new System.Drawing.Point(100, 12);
            this.txtSearch.Size = new System.Drawing.Size(600, 20);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));

            this.labelSearch.Text = "Search:";
            this.labelSearch.ForeColor = System.Drawing.Color.White;
            this.labelSearch.Location = new System.Drawing.Point(12, 15);
            this.labelSearch.Size = new System.Drawing.Size(80, 20);

            this.btnClose.Text = "Close";
            this.btnClose.BackColor = System.Drawing.Color.Red;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(700, 410);
            this.btnClose.Size = new System.Drawing.Size(88, 30);
            this.btnClose.TabIndex = 3;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.labelLoading.Text = "جاري التحميل...";
            this.labelLoading.ForeColor = System.Drawing.Color.White;
            this.labelLoading.Location = new System.Drawing.Point(12, 410);
            this.labelLoading.Size = new System.Drawing.Size(200, 30);
            this.labelLoading.Visible = false;
            this.labelLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));

            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.Location = new System.Drawing.Point(220, 410);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(470, 30);
            this.labelMessage.TabIndex = 4;
            this.labelMessage.Text = "";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelMessage.AutoSize = false;
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));

            this.btnBack.Text = "Back";
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(12, 12);
            this.btnBack.Size = new System.Drawing.Size(80, 30);
            this.btnBack.TabIndex = 5;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            this.btnBack.Visible = false;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelLoading);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.labelSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.flowLayoutGames);
            this.Controls.Add(this.flowLayoutPackages);
            this.Text = "Packages - ProMag Steam Games";
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;
            this.Load += new System.EventHandler(this.PackagesForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPackages;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutGames;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label labelSearch;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label labelLoading;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Button btnBack;
    }
}