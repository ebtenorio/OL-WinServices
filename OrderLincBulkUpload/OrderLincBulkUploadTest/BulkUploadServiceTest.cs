using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderLinc.BulkUploadLib;
using OrderLincBulkUploadTest.Properties;

namespace OrderLincBulkUploadTest
{
    [TestClass]
    public class BulkUploadServiceTest
    {
        BulkUploadService _bulkUploadService;

        //[ClassInitialize]
        public void MyClassInitialize()
        {
           
        }

        [TestInitialize]
        public void InitBulkUploadService()
        {

            DbConnection dbConnection = new DbConnection();

            Settings settings = Properties.Settings.Default;


            dbConnection.ServerName = settings.ServerName;
            dbConnection.DbName = settings.DbName;
            dbConnection.IsWindowsAuthentication = settings.AuthenticationType == 0 ? true : false;
            dbConnection.Username = settings.UserName;
            dbConnection.Password = settings.Password;


            _bulkUploadService = new BulkUploadService(dbConnection, "Order Linc - Bulk Data Load");
        }

        [TestMethod]
        public void Start()
        {
            _bulkUploadService.ProcessFiles();

        }

        [TestMethod]
        public void Stop()
        {
            _bulkUploadService.Stop();
        }

    }
}
