namespace OrderLincBulkDataLoadSettings
{
    partial class frmSettings
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
            this.tabConfig = new System.Windows.Forms.TabControl();
            this.tbpDBConfig = new System.Windows.Forms.TabPage();
            this.chkSqlServerAuthentication = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbpServiceConfig = new System.Windows.Forms.TabPage();
            this.btnServiceConfigSave = new System.Windows.Forms.Button();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.nudInterval = new System.Windows.Forms.NumericUpDown();
            this.nudExecuteTime = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbpLogNotification = new System.Windows.Forms.TabPage();
            this.btnLogNotificationSave = new System.Windows.Forms.Button();
            this.txtSubject = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkSendLogtoOfficeAdmin = new System.Windows.Forms.CheckBox();
            this.chkSendLogToSysAdmin = new System.Windows.Forms.CheckBox();
            this.ni = new System.Windows.Forms.NotifyIcon(this.components);
            this.cms = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiShow = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tabConfig.SuspendLayout();
            this.tbpDBConfig.SuspendLayout();
            this.tbpServiceConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExecuteTime)).BeginInit();
            this.tbpLogNotification.SuspendLayout();
            this.cms.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabConfig
            // 
            this.tabConfig.Controls.Add(this.tbpDBConfig);
            this.tabConfig.Controls.Add(this.tbpServiceConfig);
            this.tabConfig.Controls.Add(this.tbpLogNotification);
            this.tabConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabConfig.Location = new System.Drawing.Point(0, 0);
            this.tabConfig.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.SelectedIndex = 0;
            this.tabConfig.Size = new System.Drawing.Size(400, 229);
            this.tabConfig.TabIndex = 0;
            this.tabConfig.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabConfig_Selecting);
            // 
            // tbpDBConfig
            // 
            this.tbpDBConfig.Controls.Add(this.chkSqlServerAuthentication);
            this.tbpDBConfig.Controls.Add(this.btnSave);
            this.tbpDBConfig.Controls.Add(this.txtPassword);
            this.tbpDBConfig.Controls.Add(this.label4);
            this.tbpDBConfig.Controls.Add(this.txtUsername);
            this.tbpDBConfig.Controls.Add(this.label3);
            this.tbpDBConfig.Controls.Add(this.txtDatabase);
            this.tbpDBConfig.Controls.Add(this.label2);
            this.tbpDBConfig.Controls.Add(this.txtServer);
            this.tbpDBConfig.Controls.Add(this.label1);
            this.tbpDBConfig.Location = new System.Drawing.Point(4, 25);
            this.tbpDBConfig.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbpDBConfig.Name = "tbpDBConfig";
            this.tbpDBConfig.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbpDBConfig.Size = new System.Drawing.Size(392, 200);
            this.tbpDBConfig.TabIndex = 0;
            this.tbpDBConfig.Text = "Database";
            this.tbpDBConfig.UseVisualStyleBackColor = true;
            // 
            // chkSqlServerAuthentication
            // 
            this.chkSqlServerAuthentication.AutoSize = true;
            this.chkSqlServerAuthentication.Location = new System.Drawing.Point(94, 75);
            this.chkSqlServerAuthentication.Name = "chkSqlServerAuthentication";
            this.chkSqlServerAuthentication.Size = new System.Drawing.Size(172, 20);
            this.chkSqlServerAuthentication.TabIndex = 7;
            this.chkSqlServerAuthentication.Text = "Sql Server Authentication";
            this.chkSqlServerAuthentication.UseVisualStyleBackColor = true;
            this.chkSqlServerAuthentication.CheckedChanged += new System.EventHandler(this.chkSqlServerAuthentication_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(294, 165);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(87, 27);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(94, 126);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '|';
            this.txtPassword.Size = new System.Drawing.Size(286, 22);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password";
            // 
            // txtUsername
            // 
            this.txtUsername.Enabled = false;
            this.txtUsername.Location = new System.Drawing.Point(94, 98);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(286, 22);
            this.txtUsername.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Username";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(94, 36);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(286, 22);
            this.txtDatabase.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Database";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(94, 8);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(286, 22);
            this.txtServer.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server";
            // 
            // tbpServiceConfig
            // 
            this.tbpServiceConfig.Controls.Add(this.btnServiceConfigSave);
            this.tbpServiceConfig.Controls.Add(this.btnBrowseFolder);
            this.tbpServiceConfig.Controls.Add(this.label10);
            this.tbpServiceConfig.Controls.Add(this.label9);
            this.tbpServiceConfig.Controls.Add(this.nudInterval);
            this.tbpServiceConfig.Controls.Add(this.nudExecuteTime);
            this.tbpServiceConfig.Controls.Add(this.label5);
            this.tbpServiceConfig.Controls.Add(this.txtPath);
            this.tbpServiceConfig.Controls.Add(this.label7);
            this.tbpServiceConfig.Controls.Add(this.label6);
            this.tbpServiceConfig.Location = new System.Drawing.Point(4, 25);
            this.tbpServiceConfig.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbpServiceConfig.Name = "tbpServiceConfig";
            this.tbpServiceConfig.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbpServiceConfig.Size = new System.Drawing.Size(392, 200);
            this.tbpServiceConfig.TabIndex = 1;
            this.tbpServiceConfig.Text = "Service";
            this.tbpServiceConfig.UseVisualStyleBackColor = true;
            // 
            // btnServiceConfigSave
            // 
            this.btnServiceConfigSave.Location = new System.Drawing.Point(294, 165);
            this.btnServiceConfigSave.Name = "btnServiceConfigSave";
            this.btnServiceConfigSave.Size = new System.Drawing.Size(87, 27);
            this.btnServiceConfigSave.TabIndex = 3;
            this.btnServiceConfigSave.Text = "Save";
            this.btnServiceConfigSave.UseVisualStyleBackColor = true;
            this.btnServiceConfigSave.Click += new System.EventHandler(this.btnServiceConfigSave_Click);
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Location = new System.Drawing.Point(351, 8);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(33, 23);
            this.btnBrowseFolder.TabIndex = 10;
            this.btnBrowseFolder.Text = "...";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(163, 43);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(128, 16);
            this.label10.TabIndex = 9;
            this.label10.Text = "24-hour format [0-23]";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(163, 69);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 16);
            this.label9.TabIndex = 9;
            this.label9.Text = "second(s)";
            // 
            // nudInterval
            // 
            this.nudInterval.Location = new System.Drawing.Point(97, 63);
            this.nudInterval.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudInterval.Name = "nudInterval";
            this.nudInterval.Size = new System.Drawing.Size(60, 22);
            this.nudInterval.TabIndex = 2;
            this.nudInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudExecuteTime
            // 
            this.nudExecuteTime.Location = new System.Drawing.Point(97, 37);
            this.nudExecuteTime.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.nudExecuteTime.Name = "nudExecuteTime";
            this.nudExecuteTime.Size = new System.Drawing.Size(60, 22);
            this.nudExecuteTime.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Interval";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(51, 8);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(294, 22);
            this.txtPath.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 16);
            this.label7.TabIndex = 4;
            this.label7.Text = "Path";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 16);
            this.label6.TabIndex = 4;
            this.label6.Text = "Execute Time";
            // 
            // tbpLogNotification
            // 
            this.tbpLogNotification.Controls.Add(this.btnLogNotificationSave);
            this.tbpLogNotification.Controls.Add(this.txtSubject);
            this.tbpLogNotification.Controls.Add(this.label8);
            this.tbpLogNotification.Controls.Add(this.chkSendLogtoOfficeAdmin);
            this.tbpLogNotification.Controls.Add(this.chkSendLogToSysAdmin);
            this.tbpLogNotification.Location = new System.Drawing.Point(4, 25);
            this.tbpLogNotification.Name = "tbpLogNotification";
            this.tbpLogNotification.Padding = new System.Windows.Forms.Padding(3);
            this.tbpLogNotification.Size = new System.Drawing.Size(392, 200);
            this.tbpLogNotification.TabIndex = 2;
            this.tbpLogNotification.Text = "Log Notification";
            this.tbpLogNotification.UseVisualStyleBackColor = true;
            // 
            // btnLogNotificationSave
            // 
            this.btnLogNotificationSave.Location = new System.Drawing.Point(294, 165);
            this.btnLogNotificationSave.Name = "btnLogNotificationSave";
            this.btnLogNotificationSave.Size = new System.Drawing.Size(87, 27);
            this.btnLogNotificationSave.TabIndex = 14;
            this.btnLogNotificationSave.Text = "Save";
            this.btnLogNotificationSave.UseVisualStyleBackColor = true;
            this.btnLogNotificationSave.Click += new System.EventHandler(this.btnLogNotificationSave_Click);
            // 
            // txtSubject
            // 
            this.txtSubject.Location = new System.Drawing.Point(97, 8);
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.Size = new System.Drawing.Size(289, 22);
            this.txtSubject.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 8);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 16);
            this.label8.TabIndex = 12;
            this.label8.Text = "Email Subject";
            // 
            // chkSendLogtoOfficeAdmin
            // 
            this.chkSendLogtoOfficeAdmin.AutoSize = true;
            this.chkSendLogtoOfficeAdmin.Location = new System.Drawing.Point(97, 62);
            this.chkSendLogtoOfficeAdmin.Name = "chkSendLogtoOfficeAdmin";
            this.chkSendLogtoOfficeAdmin.Size = new System.Drawing.Size(170, 20);
            this.chkSendLogtoOfficeAdmin.TabIndex = 2;
            this.chkSendLogtoOfficeAdmin.Text = "Send log to Office Admin";
            this.chkSendLogtoOfficeAdmin.UseVisualStyleBackColor = true;
            // 
            // chkSendLogToSysAdmin
            // 
            this.chkSendLogToSysAdmin.AutoSize = true;
            this.chkSendLogToSysAdmin.Location = new System.Drawing.Point(97, 36);
            this.chkSendLogToSysAdmin.Name = "chkSendLogToSysAdmin";
            this.chkSendLogToSysAdmin.Size = new System.Drawing.Size(157, 20);
            this.chkSendLogToSysAdmin.TabIndex = 1;
            this.chkSendLogToSysAdmin.Text = "Send log to SysAdmin";
            this.chkSendLogToSysAdmin.UseVisualStyleBackColor = true;
            // 
            // ni
            // 
            this.ni.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ni.BalloonTipTitle = "Order Linc - Bulk Data Load";
            this.ni.ContextMenuStrip = this.cms;
            this.ni.Icon = ((System.Drawing.Icon)(resources.GetObject("ni.Icon")));
            this.ni.Text = "Order Linc - Bulk Data Load";
            this.ni.Visible = true;
            this.ni.DoubleClick += new System.EventHandler(this.ni_DoubleClick);
            // 
            // cms
            // 
            this.cms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShow,
            this.tsmiExit});
            this.cms.Name = "cms";
            this.cms.Size = new System.Drawing.Size(101, 48);
            // 
            // tsmiShow
            // 
            this.tsmiShow.Name = "tsmiShow";
            this.tsmiShow.Size = new System.Drawing.Size(100, 22);
            this.tsmiShow.Text = "Show";
            this.tsmiShow.Click += new System.EventHandler(this.tsmiShow_Click);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(100, 22);
            this.tsmiExit.Text = "Exit";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 229);
            this.Controls.Add(this.tabConfig);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "frmSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Order  Linc - Bulk Data Load Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSettings_FormClosing);
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.Resize += new System.EventHandler(this.frmSettings_Resize);
            this.tabConfig.ResumeLayout(false);
            this.tbpDBConfig.ResumeLayout(false);
            this.tbpDBConfig.PerformLayout();
            this.tbpServiceConfig.ResumeLayout(false);
            this.tbpServiceConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudExecuteTime)).EndInit();
            this.tbpLogNotification.ResumeLayout(false);
            this.tbpLogNotification.PerformLayout();
            this.cms.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabConfig;
        private System.Windows.Forms.TabPage tbpDBConfig;
        private System.Windows.Forms.TabPage tbpServiceConfig;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkSqlServerAuthentication;
        private System.Windows.Forms.NotifyIcon ni;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tbpLogNotification;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkSendLogtoOfficeAdmin;
        private System.Windows.Forms.CheckBox chkSendLogToSysAdmin;
        private System.Windows.Forms.TextBox txtSubject;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudInterval;
        private System.Windows.Forms.NumericUpDown nudExecuteTime;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Button btnServiceConfigSave;
        private System.Windows.Forms.Button btnLogNotificationSave;
        private System.Windows.Forms.ContextMenuStrip cms;
        private System.Windows.Forms.ToolStripMenuItem tsmiShow;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
    }
}

