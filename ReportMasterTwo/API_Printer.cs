using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportMasterTwo
{
    public class API_Printer
    {
        private List<TextFormat> Formats;

        public API_Printer(List<TextFormat> formats)
        {
            Formats = formats;
        }

        public List<List<API_Value>> Write(int recordCount)
        {
            string temp;
            object tempo;

            List<List<API_Value>> record_table = new List<List<API_Value>>();

            for (int i = 0; i < recordCount; i++)
            {
                record_table.Add(new List<API_Value>());

                for (int j = 0; j < Formats.Count; j++)
                {
                    tempo = Formats[j].Results[i];
                    temp = Formats[j].FormatRecord(i);

                    record_table[i].Add(new API_Value(tempo, temp));
                }
            }

            return record_table;
        }
    }
}
