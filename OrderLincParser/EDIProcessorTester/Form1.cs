using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OrderLinc;
namespace EDIProcessorTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            OrderLincServices _orderLinc = new OrderLincServices("192.168.1.13", "OrderLinc", false, "hsn_builder", "h$n2oo9");

       OrderLinc.EDIFileProcessor.OrderTOOWriter mOrder = new OrderLinc.EDIFileProcessor.OrderTOOWriter(_orderLinc);
          mOrder.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {

        }
    }
}
