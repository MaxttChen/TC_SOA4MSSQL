using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TC_SOA4MSSQL
{
    public class ITEM
    {

        public static SqlString CreateItem(string serverHost)
        {


            TCSOA4MSSQL.WebReference.Core2001DataManagementService s = new TCSOA4MSSQL.WebReference.Core2001DataManagementService();

            

            string result = string.Empty;

            //serverHost = "http://localhost:7001/tc";
            Process p = new Process();

            p.StartInfo.FileName = ("F:/TCSOA4MSSQL_cmd.exe");
            p.StartInfo.Arguments = @"http://localhost:7001/tc";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            result = p.StandardOutput.ReadToEnd();
            p.Close();

            return new SqlString(result);
        }

    }
}
