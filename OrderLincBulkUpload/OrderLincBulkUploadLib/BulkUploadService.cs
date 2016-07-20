using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using OrderLinc.IDataContracts;
using System.Timers;
using OrderLinc.DTOs;
using System.Diagnostics;
using System.IO;
using PTI;

namespace OrderLinc.BulkUploadLib
{
    public class BulkUploadService
    {
        #region Variables
        const string INPUT_FOLDER = "Input";
        const string ARCHIVE_FOLDER = "Archive";
        const string LOG_FOLDER = "Log";

        OrderLincServices _orderLinc;
        IConfigurationService _configurationService;
        ICustomerService _customerService;
        ILogService _logService;

        int _interval;

        bool _isInit;
        bool _isValid;
        bool _hasPrevalidatedError;
        
        Timer _timer;
      
        string _targetFolder;

        DTOMailConfig _mailConfig;
        DTOAccountList _adminAccount;
        DTOAccount _bgAccount;
        DTODataLoadConfig _dataLoadConfig;

        EmailSVC _emailSvc;
        List<string> _files;
        DbConnection _dbConnection;
        DateTime _lastRun;
        #endregion

        #region Constructor and Service Initialization

        public BulkUploadService(DbConnection dbConnection, string eventSource)
        {
            _dbConnection = dbConnection;

            EventSourceName = eventSource;
            Init();

            InitAndCheckDataUploadConfig();

            PreValidateTargetFolders();
            _lastRun = DateTime.MinValue;

            InitFilenames();
        }

        private void InitFilenames()
        {
            _files = new List<string>();

            _files.Add("Account");
            _files.Add("Customer");
            _files.Add("CustomerSalesRep");
            _files.Add("Product");
            _files.Add("Provider");
            _files.Add("ProviderWarehouse");
            _files.Add("ProviderCustomer");
            _files.Add("ProviderProduct");

        }

        private void Init()
        {
            try
            {
                //string dbServer = "192.168.50.24";// GetSettings("ServerName");// ConfigurationManager.AppSettings[""];
                //string dbName = "OL3_TestBulkUpload";//GetSettings("DbName");
                //string dbUser = "oluser";//GetSettings("UserName");
                //string dbPassword = "pti@1234";// GetSettings("Password");
                //string dbAuthentication = "1";// GetSettings("AuthenticationType");

                if (_dbConnection == null)
                {
                    throw new ApplicationException("Invalid database configuration or database connection has not been set.");
                }

                _interval = 10;// int.Parse(GetSettings("Interval"));
                _orderLinc = new OrderLincServices(_dbConnection.ServerName,
                    _dbConnection.DbName, _dbConnection.IsWindowsAuthentication, _dbConnection.Username, _dbConnection.Password);

                _logService = _orderLinc.LogService;
                _configurationService = _orderLinc.ConfigurationService;
                _customerService = _orderLinc.CustomerService;

                _isInit = true;
            }
            catch (Exception ex)
            {
                LogMe(ex);
                throw ex;
            }
        }

        private string GetSettings(string appName)
        {
            
            return ConfigurationManager.AppSettings[appName];
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            if (_isInit == false) return;

            if (_interval == 0) _interval = 60;

            _timer = new Timer();
            _timer.Interval = 1000 * _interval;
            _timer.Enabled = true;

            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
        }


        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Stop();
            }
        }

        #endregion
        
        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isInit == false || (_isValid == false || _hasPrevalidatedError || _dataLoadConfig == null))
            {
                _timer.Stop();

                return;
            }

            if (_dataLoadConfig.DataLoadTime >= 0)
            {
                if (e.SignalTime.Hour != _dataLoadConfig.DataLoadTime) return;
                else if (_lastRun.Date.Equals(DateTime.Today) && _lastRun.Hour == e.SignalTime.Hour) return;
            }
            _timer.Stop();
            _lastRun = DateTime.Now;
            ProcessFiles();

            _timer.Start();
        }

        public string EventSourceName { get; set; }
                
        private void PreValidateTargetFolders()
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(_targetFolder))
                {
                    string inputDir = Path.Combine(dir, INPUT_FOLDER);
                    if (!Directory.Exists(inputDir))
                        Directory.CreateDirectory(inputDir);

                    string archive = Path.Combine(dir, ARCHIVE_FOLDER);
                    if (!Directory.Exists(archive))
                        Directory.CreateDirectory(archive);

                    string log = Path.Combine(dir, ARCHIVE_FOLDER);
                    if (!Directory.Exists(log))
                        Directory.CreateDirectory(log);

                }
                _hasPrevalidatedError = false;
            }
            catch (Exception ex)
            {

                _hasPrevalidatedError = true;
                LogMe(ex);

                throw;
            }

        }

        public void ProcessFiles()
        {
            try
            {
                if (_isInit == false || _isValid == false || _hasPrevalidatedError) return;

                DTOSalesOrgList salesOrgList = _customerService.SalesOrgList();

                if (salesOrgList == null || (salesOrgList != null && salesOrgList.Count == 0))
                {
                    LogMe("No sales org list found.", EventLogEntryType.Error);

                    return;
                }


                foreach (string salesOrgDir in Directory.GetDirectories(_targetFolder))
                {
                    string folderDate = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    string salesOrgShortName = Path.GetFileNameWithoutExtension(salesOrgDir);

                    DTOSalesOrg salesOrg = _customerService.SalesOrgListBySalesOrgShortName(salesOrgShortName);  // salesOrgList.Where(s => s.SalesOrgShortName != null && s.SalesOrgCode.Equals(salesOrgShortName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    
                    ErrorStringBuilder erBlr = new ErrorStringBuilder();
                   
                    if (salesOrg == null)
                    {
                        erBlr.AddError("Sales Org '{0}' does not exist.", salesOrgShortName);
                        continue;
                    }

                    erBlr.Add(string.Format("Processing {0}...", salesOrg.SalesOrgName));
                    erBlr.Add(string.Format("Start time : {0}", DateTime.Now));

                    string salesOrgInputPath = Path.Combine(salesOrgDir, INPUT_FOLDER);
                    string salesOrgArchivePath = Path.Combine(salesOrgDir, ARCHIVE_FOLDER, folderDate);
                    string salesOrgLogPath = Path.Combine(salesOrgDir, LOG_FOLDER, folderDate);

                    if (!Directory.Exists(salesOrgArchivePath))
                        Directory.CreateDirectory(salesOrgArchivePath);

                    if (!Directory.Exists(salesOrgLogPath))
                        Directory.CreateDirectory(salesOrgLogPath);

                    if (!Directory.Exists(salesOrgInputPath))
                    {
                        erBlr.AddWarning("No input folder for sales org '{0}'.", salesOrgDir);
                        continue;
                    }
                    //string[] files = Directory.GetFiles(salesOrgInputPath, "*.csv");

                    foreach (string s in _files)
                    {
                        string file = Path.Combine(salesOrgInputPath, string.Format("{0}.csv", s));

                        erBlr.AddSeparator();
                        erBlr.Add(string.Empty);
                        erBlr.Add(string.Format("Processing {0}.csv file...",  s));
                        erBlr.Add(string.Format("Start Time: {0}" , DateTime.Now));
                        erBlr.Add(string.Empty);
                        if (!File.Exists(file))
                        {
                            erBlr.AddWarning("File '{0}' not found.", Path.GetFileNameWithoutExtension(file));
                            continue;
                        }

                        //string fileName = Path.GetFileNameWithoutExtension(file);

                        DataUpload uploader = GetUploader(file);

                        if (uploader == null)
                        {
                            erBlr.AddError("File {0} not handled.", s);
                            erBlr.Add(string.Format("End Time: {0}", DateTime.Now));
                            erBlr.Add(string.Format("End of {0}.csv", s));
                            erBlr.Add(string.Empty);
                            continue;
                        }

                        uploader.SalesOrgID = salesOrg.SalesOrgID;
                        uploader.UserID = _bgAccount.AccountID;

                        uploader.Upload();

                        if (uploader.HasError == true)
                        {
                            erBlr.AddError(uploader.Error);
                            File.Copy(file, Path.Combine(salesOrgArchivePath, Path.GetFileName(file)));
                        }
                        else
                        {
                            File.Move(file, Path.Combine(salesOrgArchivePath, Path.GetFileName(file)));
                        }
                        erBlr.Add(string.Empty);
                        erBlr.Add(string.Format("Total lines: {0}",uploader.LineCount));
                        erBlr.Add(string.Format("Processed lines: {0}", uploader.LineCountProcessed));
                        erBlr.Add(string.Format("End Time: {0}", DateTime.Now));
                        erBlr.Add(string.Empty);
                        erBlr.Add(string.Format("End of {0}.csv", s));
                        erBlr.Add(string.Empty);
                    } // foreach

                    erBlr.Add(string.Format("Complete time: {0}", DateTime.Now));

                    string message = erBlr.ToString();
                    WriteLog(Path.Combine(salesOrgLogPath, string.Format("{0}.log", salesOrgShortName)), message);

                    EmailLog(salesOrg, message);

                } // foreach
            }
            catch (Exception ex)
            {
                LogMe(ex);
            }
        }

        void EmailLog(DTOSalesOrg salesOrg, string msg)
        {
            try
            {
                string subject = string.IsNullOrEmpty(_dataLoadConfig.Subject) ? "Data Load Log" : _dataLoadConfig.Subject;
                
                string message = msg + "\r\n\r\n\r\n" + _dataLoadConfig.Footer;

                if (_dataLoadConfig != null && _dataLoadConfig.SendLogToSystemAdmin)
                {
                    foreach (var accnt in _adminAccount)
                    {
                        string email = accnt.Email ?? string.Empty;
                        if (!string.IsNullOrEmpty(email))
                            _emailSvc.SendMail(email, string.Empty, subject, message);
                    }
                }
            
                if (salesOrg.ContactID > 0)
                {
                    //DTOContact contact = _customerService.ContactListByID(salesOrg.ContactID);

                    //if (contact == null) return;

                    //if (string.IsNullOrEmpty(contact.Email)) return;
                    //SendMail(contact.Email, msg, "Data upload");

                    if (_dataLoadConfig.SendLogToOfficeAdmin)
                    {
                        DTOContactList contactList = _customerService.ContactListByAccountTypeSalesOrgID(salesOrg.SalesOrgID, AccountType.OfficeAdmin);

                        if (contactList.Count == 0) return;

                        foreach (var contact in contactList)
                        {
                            if (string.IsNullOrEmpty(contact.Email)) continue;
                            string email = contact.Email;
                            //email = "l.gongob@paperlesstrail.net";

                            _emailSvc.SendMail(email, string.Empty, subject, message);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMe(ex);
            }
        }

        void WriteLog(string logPath, string logMessage)
        {
            try
            {
                using (FileStream fs = new FileStream(logPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] b = Encoding.UTF8.GetBytes(logMessage);
                    fs.Write(b, 0, b.Length);
                }
            }
            catch (Exception ex)
            {
                LogMe(string.Format("Error writing log '{0}' to {1}", ex.Message, logPath), EventLogEntryType.Error);
            }
        }

        private DataUpload GetUploader(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);

            DataUpload uploader = null;

            switch (filename.ToLower())
            {
                case "provider":
                    uploader = new ProviderUpload(_orderLinc, path);
                    break;
                case "product":
                    uploader = new ProductUploader(_orderLinc, path);
                    break;
                case "providerproduct":
                    uploader = new ProviderProductUploader(_orderLinc, path);
                    break;

                case "customer":
                    uploader = new CustomerUploader(_orderLinc, path);
                    break;

                case "providercustomer":
                    uploader = new ProviderCustomerUploader(_orderLinc, path);
                    break;

                case "account":
                    uploader = new AccountUploader(_orderLinc, path);
                    break;

                case "customersalesrep":
                    uploader = new CustomerSalesRepUploader(_orderLinc, path);
                    break;
                case "providerwarehouse":
                    uploader = new ProviderWarehouseUploader(_orderLinc, path);
                    break;
            }
            return uploader;
        }

        private void CheckAndCreateDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public bool HasError
        {
            get
            {
                return (_isValid == false ) || (_isInit == false ) || (_hasPrevalidatedError == true);
            }
        }
        /// <summary>
        /// Initialize account, mail configuration, target dir
        /// </summary>
        private void InitAndCheckDataUploadConfig()
        {
            if (_isInit == false) return;
            try
            {
                _adminAccount = _orderLinc.AccountService.AccountListByAccountTypeID(1);

                var bgAccounts = _orderLinc.AccountService.AccountListByAccountTypeID(5);

                if (bgAccounts.Count > 0) _bgAccount = bgAccounts[0];
                else
                {
                    LogMe("No background process account has found, please add account for background process.", EventLogEntryType.Error);
                    _isValid = false;
                    return;
                }

                _mailConfig = _configurationService.SYSConfigMail();

                if (_mailConfig == null)
                {
                    LogMe("One or more email configuration is missing.", EventLogEntryType.Error);
                    _isValid = false;
                }

                _dataLoadConfig = _configurationService.SYSConfigDataLoad();
                if (_dataLoadConfig == null)
                {
                    LogMe("Error reading data load configuration.", EventLogEntryType.Error);
                    _isValid = false;
                }
                else
                {
                    _targetFolder = _dataLoadConfig.DataLoadPath;
                    _interval = _dataLoadConfig.Interval;
                }

                
                _emailSvc = new EmailSVC(_mailConfig.HostName, _mailConfig.Port, _mailConfig.UseDefaultCredential, _mailConfig.Username, _mailConfig.Password, _mailConfig.SenderEmail);

                if (!Directory.Exists(_targetFolder))
                {
                    Directory.CreateDirectory(_targetFolder);
                }

                _isValid = true;
            }
            catch (Exception ex)
            {
                LogMe(ex);
                throw;
            }

        }

        #region Log

        private void LogMe(Exception ex)
        {
            string message = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);

            LogMe(message, EventLogEntryType.Error);
        }

        private void LogMe(string msg, EventLogEntryType eventType)
        {
            if (_logService != null && _isInit)
            {
                _logService.LogSave(EventSourceName, msg, _adminAccount == null || (_adminAccount != null && _adminAccount.Count == 0) ? 99 : _adminAccount[0].AccountID);


                if (_isValid)
                {
                    string message = msg + "\r\n" + "\r\n" + "\r\n" + "\r\n" + _dataLoadConfig.Footer;
                    SendMailError(message, string.IsNullOrEmpty(_dataLoadConfig.Subject) ? "Data Load Service Error" : _dataLoadConfig.Subject);
                }
            }

            try
            {
                EventLog.WriteEntry(EventSourceName, msg, eventType);
            }
            catch { }
        }

        private void SendMailError(string message, string subject)
        {
            if (_isInit == false || _isValid == false) return;

            try
            {
                foreach (var accnt in _adminAccount)
                {
                    _emailSvc.SendMail(accnt.Email, string.Empty, subject, message);
                }
           
            }
            catch (Exception ex)
            {
                // don't call LogMe, this might result in recursive call.
                try
                {
                    EventLog.WriteEntry(EventSourceName, string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace), EventLogEntryType.Error);
                }
                catch
                {
                    throw;
                }

            }
        }

     #endregion

    }


}
