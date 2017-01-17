using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportMasterTwo;

namespace ReportMasterTester
{
    class Program
    {
        static void Main(string[] args)
        {
            ReportMaster rm = new ReportMaster(args[0], args[1], args[2]);
            rm.Run();
        }
    }
}
