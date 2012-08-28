using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Data.Odbc;

namespace ReportMasterTwo
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 2)
                {   // ReportMasterTwo ReportName OutputName
                    string oconnS = @"Dsn=SOTAMAS90";

                    ReportMaster rm = new ReportMaster(args[0], args[1], oconnS);
                    rm.Run();
                }
                else
                {
                    System.Console.WriteLine("Usage:");
                    System.Console.WriteLine("ReportMasterTwo ReportFileName OutputFileName");
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.ToString());
                System.Console.WriteLine("YOYOYO!");

                string line = System.Console.ReadLine();
                System.Console.WriteLine(line + " you said");
            }
        }
    }
}