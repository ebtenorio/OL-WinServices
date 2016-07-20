using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using OrderLinc.EDIFileProcessor;
using OrderLinc;


namespace OrderAppParserWindowsService
{
    public partial class OrderAppParserWindowsService : ServiceBase
    {
        OrderTORReader _orderReader;
        OrderTOOWriter _orderWriter;

        
      

        OrderLincServices _orderLinc;

        public OrderAppParserWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry("OrderAppParserService", "OnStart Main Process Invoked");
                Properties.Settings dbSettings = new Properties.Settings();

                _orderLinc = new OrderLincServices(dbSettings.DBServerName,
                                                              dbSettings.DBName,
                                                              dbSettings.IsWindowAuthentication,
                                                              dbSettings.Username,
                                                              dbSettings.Password);
                _orderReader = new OrderTORReader(_orderLinc);

                _orderWriter = new OrderTOOWriter(_orderLinc);


                _orderReader.Start();
                _orderWriter.Start();

            }
            catch (Exception e)
            {
                EventLog.WriteEntry("OrderAppParserService", e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine + e.InnerException.ToString(), EventLogEntryType.Information);
                throw e;
            }
        }

        protected override void OnStop()
        {
            try
            {
                EventLog.WriteEntry("OrderAppParserService", "stopping OrderLinc Parser service.");
                _orderReader.Stop();
                _orderWriter.Stop();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}