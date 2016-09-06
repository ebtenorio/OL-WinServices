using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using OrderLinc.NotificationLib;
using OrderLinc.DTOs;
using OrderLinc.IDataContracts;

namespace OrderLinc.NotificationWinService
{
    public partial class NotifyService : ServiceBase
    {

        OrderNotification _orderNotification;
      

        public NotifyService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _orderNotification = new OrderNotification(Properties.Settings.Default.ServerName, Properties.Settings.Default.DbName, Properties.Settings.Default.AuthenticationType, Properties.Settings.Default.UserName, Properties.Settings.Default.Password);

                int time = 0;

                DTOSYSConfig mConfig = _orderNotification.ServiceInterval;              

                if (mConfig != null)
                {
                    bool timeResult = int.TryParse(mConfig.ConfigValue.ToString().Trim(), out time);

                    if (timeResult == true)
                    {


                        if (time == 0)
                        {
                            _orderNotification.LogMe("Invalid  NotifyInterval Config Value in the configuration table", EventLogEntryType.Error);
                            EventLog.WriteEntry("OrderLinc", "Invalid  NotifyInterval Config Value in the configuration table.", EventLogEntryType.Information);
                        }
                        else
                        {
                            time = (time * 1000);
                            _orderNotification.LogMe("Service Timer has been successfully set to Time Interval = " + time, EventLogEntryType.Information);
                            _orderNotification.StartService(double.Parse(time.ToString()));
                        }
                    }
                    else
                    {
                        _orderNotification.LogMe("Invalid  NotifyInterval Config Value in the configuration table.", EventLogEntryType.Error);
                        EventLog.WriteEntry("OrderLinc", "Invalid  NotifyInterval Config Value in the configuration table.", EventLogEntryType.Error);
                    }

                }
                else
                {
                    _orderNotification.LogMe("Cannot find NotifyInterval configKey in the configuration table.", EventLogEntryType.Error);
                    EventLog.WriteEntry("OrderLinc", "Cannot find NotifyInterval configKey in the configuration table.", EventLogEntryType.Error);

                }

            }
            catch (Exception ex)
            {
                _orderNotification.LogMe("Error On service start - ." + ex.Message, EventLogEntryType.Information);
                EventLog.WriteEntry("OrderLinc", "Error On service start - ." + ex.Message, EventLogEntryType.Information);

            }
        }

        protected override void OnStop()
        {
            
            _orderNotification.StopService();
        }



       
    }
}
