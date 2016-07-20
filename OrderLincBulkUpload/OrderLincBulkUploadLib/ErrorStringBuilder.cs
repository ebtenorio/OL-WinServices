using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderLinc.BulkUploadLib
{
    public class ErrorStringBuilder
    {
        StringBuilder blr;

        public ErrorStringBuilder()
        {
            blr = new StringBuilder(100);
        }

        public void AddError(string errorMsg)
        {
            AddMsgLine(errorMsg);
        }

        public void Add(string message)
        {
            AddMsgLine(message);
        }
        public void AddError(string errorMsg, params object[] paramValue)
        {
            AddMsgLine(string.Format("" + errorMsg, paramValue));
        }

        public void AddWarning(string warningMsg)
        {
            AddMsgLine("Warning : " + warningMsg);

        }

        public void AddWarning(string warningMsg, params object[] paramValue)
        {
            AddMsgLine(string.Format("Warning : " + warningMsg, paramValue));
        }

        private void AddMsgLine(string msg)
        {
            if (blr.Length > 0) blr.Append("\r\n");

            blr.Append(msg);
        }
        public override string ToString()
        {
            return blr.ToString();
        }

        public bool IsEmpty
        {
            get
            {
                return (blr.Length == 0);
            }
        }

        public void AddSeparator()
        {
            if (blr.Length > 0) blr.Append("\r\n");
            blr.Append(new string('-', 200));
        }
    }
}
