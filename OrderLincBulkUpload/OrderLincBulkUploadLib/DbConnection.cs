using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderLinc.BulkUploadLib
{
    public class DbConnection
    {
        public string ServerName { get; set; }

        public string DbName { get; set; }

        public bool IsWindowsAuthentication { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

    }
}
