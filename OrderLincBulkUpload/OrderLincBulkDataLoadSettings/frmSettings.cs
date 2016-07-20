using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using OrderLinc.DTOs;

namespace OrderLincBulkDataLoadSettings
{
    public partial class frmSettings : Form
    {

        Configuration _config;
        OrderLinc.OrderLincServices _orderLincServices;
        DTOSYSConfigList _configList;
        bool exit;

        public frmSettings()
        {
            InitializeComponent();
        }

        private void chkSqlServerAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            txtUsername.Enabled = chkSqlServerAuthentication.Checked;
            txtPassword.Enabled = chkSqlServerAuthentication.Checked;

            if (chkSqlServerAuthentication.Checked)
            {
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (Directory.Exists(txtPath.Text))
            {
                fbd.SelectedPath = txtPath.Text;
            }

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = fbd.SelectedPath;
            }
            fbd.Dispose();
        }

        private void ReadServiceConfiguration()
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "OrderLincBulkDataLoad.exe");
           
            //string path = Path.Combine(@"C:\Program Files (x86)\PTI\Order Linc - Bulk Data Load", "OrderLincBulkDataLoad.exe");
            //ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            //map.ExeConfigFilename = path;

            //Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            _config = ConfigurationManager.OpenExeConfiguration(path);
            ConfigurationSectionGroup secgroup = _config.GetSectionGroup("userSettings");

            if (secgroup == null || (secgroup != null && secgroup.Sections.Count == 0))
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }

            ClientSettingsSection settingSection = (ClientSettingsSection)secgroup.Sections[0];


            SettingElement settingEl = settingSection.Settings.Get("ServerName");
            if (settingEl == null)
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }
            txtServer.Text = settingEl.Value.ValueXml.InnerText;

            settingEl = settingSection.Settings.Get("DbName");
            if (settingEl == null)
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }
            txtDatabase.Text = settingEl.Value.ValueXml.InnerText;

            settingEl = settingSection.Settings.Get("AuthenticationType");
            if (settingEl == null)
            {
                MessageBox.Show("Could not read configuration.", AppName);
                return;
            }
            string val = settingEl.Value.ValueXml.InnerText;

            chkSqlServerAuthentication.Checked = (val == "1");

            settingEl = settingSection.Settings.Get("UserName");
            if (settingEl == null)
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }
            txtUsername.Text = settingEl.Value.ValueXml.InnerText;

            settingEl = settingSection.Settings.Get("Password");
            if (settingEl == null)
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }
            txtPassword.Text = settingEl.Value.ValueXml.InnerText;


        }

        private void SaveConfiguration()
        {
            if (_config == null)
            {
                MessageBox.Show("No configuration found.", AppName);
                return;
            }
            ConfigurationSectionGroup sectionGroup = _config.SectionGroups["userSettings"];
            if (sectionGroup == null || (sectionGroup != null && sectionGroup.Sections.Count == 0))
            {
                MessageBox.Show("Could not read the configuration.", AppName);
                return;
            }


            ClientSettingsSection clientSection = (ClientSettingsSection)sectionGroup.Sections[0];

            SettingElement el = clientSection.Settings.Get("ServerName");
            el.Value.ValueXml.InnerText = txtServer.Text;

            el = clientSection.Settings.Get("DbName");
            el.Value.ValueXml.InnerText = txtDatabase.Text;


            el = clientSection.Settings.Get("UserName");
            el.Value.ValueXml.InnerText = txtUsername.Text;


            el = clientSection.Settings.Get("Password");
            el.Value.ValueXml.InnerText = txtPassword.Text;


            el = clientSection.Settings.Get("AuthenticationType");
            el.Value.ValueXml.InnerText = chkSqlServerAuthentication.Checked ? "1" : "0";


            _config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("userSettings");

        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            ReadServiceConfiguration();
        }

        private void ni_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            ni.Visible = false;
        }

        private void frmSettings_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                try
                {
                    ni.Visible = true;
                    ni.BalloonTipText = "Configuration";
                    ni.ShowBalloonTip(500);
                    this.ShowInTaskbar = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exit) return;

            if (MessageBox.Show("Do want to minimize to system tray?", AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                return;

            e.Cancel = true;
            WindowState = FormWindowState.Minimized;
        }

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            ni.Visible = false;
        }

        private string AppName = Application.ProductName;

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            exit = true;
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text))
            {
                MessageBox.Show("Server name is required.", AppName);
                return;
            }

            if (string.IsNullOrEmpty(txtDatabase.Text))
            {
                MessageBox.Show("Database name is required.", AppName);
                return;
            }

            if (chkSqlServerAuthentication.Checked && (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text)))
            {
                MessageBox.Show("Username and password is required for Sql Server authentication.", AppName);
                return;
            }

            SaveConfiguration();
        }

        private void ReadFromDBConfig()
        {
            try
            {
                _orderLincServices = new OrderLinc.OrderLincServices(txtServer.Text, txtDatabase.Text, chkSqlServerAuthentication.Checked == false, txtUsername.Text, txtPassword.Text);

                _configList = _orderLincServices.ConfigurationService.SYSConfigList();

                DTOSYSConfig c = _configList.Where(p => p.ConfigKey == "").FirstOrDefault();
                int intVal;


                if (c != null)
                {
                    txtPath.Text = c.ConfigValue;
                }
                c = _configList.Where(p => p.ConfigKey == "DataLoadPath").FirstOrDefault();
                if (c != null)
                {
                    txtPath.Text = c.ConfigValue;
                }
                c = _configList.Where(p => p.ConfigKey == "DataLoadTime").FirstOrDefault();
                if (c != null)
                {
                    int.TryParse(c.ConfigValue, out intVal);
                    nudExecuteTime.Text = intVal.ToString();

                }

                c = _configList.Where(p => p.ConfigKey == "DataLoadSendLogToSysAdmin").FirstOrDefault();
                if (c != null)
                {

                    chkSendLogToSysAdmin.Checked = c.ConfigValue.Trim() == "1" ? true : false;

                }
                c = _configList.Where(p => p.ConfigKey == "DataLoadSendLogToOfficeAdmin").FirstOrDefault();
                if (c != null)
                {
                    chkSendLogtoOfficeAdmin.Checked = c.ConfigValue.Trim() == "1" ? true : false;

                }
                c = _configList.Where(p => p.ConfigKey == "DataLoadInterval").FirstOrDefault();
                if (c != null)
                {
                    int.TryParse(c.ConfigValue, out intVal);
                    nudInterval.Text = intVal.ToString();
                }
                c = _configList.Where(p => p.ConfigKey == "DataLoadSubject").FirstOrDefault();
                if (c != null)
                {
                    txtSubject.Text = c.ConfigValue;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppName);
                tabConfig.SelectedIndex = 0;
            }
        }

        private void btnServiceConfigSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPath.Text) || (!string.IsNullOrEmpty(txtPath.Text) && !Directory.Exists(txtPath.Text)))
            {
                MessageBox.Show("Invalid data load directory", AppName); return;
            }
            if (nudInterval.Value == 0)
            {
                MessageBox.Show("Interval must be greater than zero.", AppName); return;
            }


            try
            {
                DTOSYSConfig c = _configList.Where(p => p.ConfigKey == "DataLoadPath").FirstOrDefault();

                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = txtPath.Text;
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);


                c = _configList.Where(p => p.ConfigKey == "DataLoadTime").FirstOrDefault();
                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = int.Parse(nudExecuteTime.Value.ToString()).ToString();
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);

                c = _configList.Where(p => p.ConfigKey == "DataLoadInterval").FirstOrDefault();
                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = int.Parse(nudInterval.Value.ToString()).ToString();
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppName);
            }
        }

        private void btnLogNotificationSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSubject.Text))
            {
                MessageBox.Show("Email subject is required", AppName); return;
            }

            try
            {
                DTOSYSConfig c = _configList.Where(p => p.ConfigKey == "DataLoadSubject").FirstOrDefault();

                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = txtSubject.Text;
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);


                c = _configList.Where(p => p.ConfigKey == "DataLoadSendLogToSysAdmin").FirstOrDefault();
                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = chkSendLogToSysAdmin.Checked ? "1" : "0";
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);

                c = _configList.Where(p => p.ConfigKey == "DataLoadSendLogToOfficeAdmin").FirstOrDefault();
                if (c == null)
                {
                    MessageBox.Show("Invalid config key.", AppName); return;
                }
                c.ConfigValue = chkSendLogtoOfficeAdmin.Checked ? "1" : "0";
                _orderLincServices.ConfigurationService.SYSConfigSaveRecord(c);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppName);
            }
        }

        private void tabConfig_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text) || string.IsNullOrEmpty(txtDatabase.Text))
            {
                MessageBox.Show("No database config.", AppName);
                e.Cancel = true;
                return;
            }

            if (chkSqlServerAuthentication.Checked)
            {
                if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Username or password is required for Sql Authentication.", AppName);
                    return;
                    e.Cancel = true;
                }
            }

            if (tabConfig.SelectedIndex == 1 || tabConfig.SelectedIndex == 2)
            {
                ReadFromDBConfig();
            }
        }

    }
}
