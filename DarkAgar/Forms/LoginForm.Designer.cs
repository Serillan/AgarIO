namespace DarkAgar.Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LoginButton = new System.Windows.Forms.Button();
            this.LoginTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.IPAddressTextBox = new System.Windows.Forms.TextBox();
            this.AdressLabel = new System.Windows.Forms.Label();
            this.ServerListBox = new System.Windows.Forms.ListBox();
            this.HelpLabel = new System.Windows.Forms.Label();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoginButton
            // 
            this.LoginButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LoginButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LoginButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LoginButton.Location = new System.Drawing.Point(372, 262);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(176, 36);
            this.LoginButton.TabIndex = 0;
            this.LoginButton.Text = "Login and play";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // LoginTextBox
            // 
            this.LoginTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LoginTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LoginTextBox.Location = new System.Drawing.Point(353, 316);
            this.LoginTextBox.Name = "LoginTextBox";
            this.LoginTextBox.Size = new System.Drawing.Size(215, 30);
            this.LoginTextBox.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.HelpLabel);
            this.panel1.Controls.Add(this.InfoLabel);
            this.panel1.Controls.Add(this.LoginTextBox);
            this.panel1.Controls.Add(this.LoginButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(955, 647);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.IPAddressTextBox);
            this.panel2.Controls.Add(this.AdressLabel);
            this.panel2.Controls.Add(this.ServerListBox);
            this.panel2.Location = new System.Drawing.Point(460, 466);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(495, 169);
            this.panel2.TabIndex = 7;
            // 
            // IPAddressTextBox
            // 
            this.IPAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAddressTextBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.IPAddressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.IPAddressTextBox.Location = new System.Drawing.Point(19, 122);
            this.IPAddressTextBox.Multiline = true;
            this.IPAddressTextBox.Name = "IPAddressTextBox";
            this.IPAddressTextBox.Size = new System.Drawing.Size(368, 32);
            this.IPAddressTextBox.TabIndex = 5;
            this.IPAddressTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.IPAddressTextBox_MouseClick);
            // 
            // AddressLabel
            // 
            this.AdressLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.AdressLabel.AutoSize = true;
            this.AdressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.AdressLabel.ForeColor = System.Drawing.SystemColors.MenuText;
            this.AdressLabel.Location = new System.Drawing.Point(134, 16);
            this.AdressLabel.Name = "AddressLabel";
            this.AdressLabel.Size = new System.Drawing.Size(193, 29);
            this.AdressLabel.TabIndex = 6;
            this.AdressLabel.Text = "Server Address";
            // 
            // ServerListBox
            // 
            this.ServerListBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ServerListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ServerListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ServerListBox.FormattingEnabled = true;
            this.ServerListBox.ItemHeight = 20;
            this.ServerListBox.Items.AddRange(new object[] {
            "localhost",
            "gameserver.northeurope.cloudapp.azure.com",
            "own IP "});
            this.ServerListBox.Location = new System.Drawing.Point(19, 48);
            this.ServerListBox.Name = "ServerListBox";
            this.ServerListBox.Size = new System.Drawing.Size(450, 80);
            this.ServerListBox.TabIndex = 4;
            // 
            // HelpLabel
            // 
            this.HelpLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.HelpLabel.AutoSize = true;
            this.HelpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.HelpLabel.Location = new System.Drawing.Point(318, 375);
            this.HelpLabel.Name = "HelpLabel";
            this.HelpLabel.Size = new System.Drawing.Size(296, 100);
            this.HelpLabel.TabIndex = 3;
            this.HelpLabel.Text = "Use mouse to control the cell.\r\nPress Space to split.\r\nPress W to eject some mass" +
    ".\r\n\r\n";
            // 
            // InfoLabel
            // 
            this.InfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.InfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.InfoLabel.ForeColor = System.Drawing.SystemColors.MenuText;
            this.InfoLabel.Location = new System.Drawing.Point(3, 564);
            this.InfoLabel.MinimumSize = new System.Drawing.Size(300, 0);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(333, 74);
            this.InfoLabel.TabIndex = 2;
            this.InfoLabel.Text = "\r\n";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(955, 647);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Name = "LoginForm";
            this.Text = "DarkAgar";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoginButton;
        public System.Windows.Forms.TextBox LoginTextBox;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.Label HelpLabel;
        public System.Windows.Forms.ListBox ServerListBox;
        public System.Windows.Forms.TextBox IPAddressTextBox;
        private System.Windows.Forms.Label AdressLabel;
        private System.Windows.Forms.Panel panel2;
    }
}