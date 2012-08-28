using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportMasterTwo
{
    public class API_Value
    {
        public object val;
        public string formatted_val;
        public Type val_type;

        public API_Value(object v, string form)
        {
            val = v;
            formatted_val = form;
            val_type = v.GetType();
        }
    }
}
