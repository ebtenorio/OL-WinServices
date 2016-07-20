using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using OrderLinc.BulkUploadLib.Properties;

namespace OrderLinc.BulkUploadLib
{
    public partial class OrderLincBulkUploadService : ServiceBase
    {
        BulkUploadService _bulkUpload;

        public OrderLincBulkUploadService()
        {
            InitializeComponent();

          
        }

        private void Init()
        {
            //if (_bulkUpload == null)
           

        }
        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();

            DbConnection dbConnection = new DbConnection();

            Settings settings = Properties.Settings.Default;


            dbConnection.ServerName = settings.ServerName;
            dbConnection.DbName = settings.DbName;
            dbConnection.IsWindowsAuthentication = settings.AuthenticationType == 0 ? true : false;
            dbConnection.Username = settings.UserName;
            dbConnection.Password = settings.Password;

            _bulkUpload = new BulkUploadService(dbConnection, this.ServiceName);
            
            if (_bulkUpload.HasError)
            {
                throw new ApplicationException("Please check log.");
            }
            _bulkUpload.Start();
        }

        protected override void OnStop()
        {
            if (_bulkUpload == null) return;
            _bulkUpload.Stop();
        }


    }
}
