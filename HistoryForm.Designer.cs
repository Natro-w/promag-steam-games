namespace ProMag_Steam_Games
{
    partial class HistoryForm
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
            this.flowLayoutGames = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.labelMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.flowLayoutGames.AutoScroll = true;
            this.flowLayoutGames.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutGames.Name = "flowLayoutGames";
            this.flowLayoutGames.Size = new System.Drawing.Size(776, 380);
            this.flowLayoutGames.TabIndex = 0;
            this.flowLayoutGames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));

            this.btnClose.Text = "Close";
            this.btnClose.BackColor = System.Drawing.Color.Red;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(700, 410);
            this.btnClose.Size = new System.Drawing.Size(88, 30);
            this.btnClose.TabIndex = 1;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.Location = new System.Drawing.Point(12, 410);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(676, 30);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelMessage.AutoSize = false;
            this.labelMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.flowLayoutGames);
            this.Text = "History - ProMag Steam Games";
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;
            this.Load += new System.EventHandler(this.HistoryForm_Load);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.FlowLayoutPanel flowLayoutGames;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label labelMessage;
    }
}