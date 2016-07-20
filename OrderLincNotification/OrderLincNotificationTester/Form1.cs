using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace OrderLincNotificationTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            OrderLinc.NotificationLib.OrderNotification mWS = new OrderLinc.NotificationLib.OrderNotification("192.168.1.189", "OrderLinc", "1", "hsn_builder", "h$n2oo9");
            mWS.StartService(100);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            OrderLinc.NotificationLib.OrderNotification mWS = new OrderLinc.NotificationLib.OrderNotification("192.168.1.189", "OrderLinc", "1", "hsn_builder", "h$n2oo9");
             mWS.StopService();
        }
    }
}
