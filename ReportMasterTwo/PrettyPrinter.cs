using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ReportMasterTwo
{
    public class PrettyPrinter
    {
        static int linesPrinted = 0;
        static int charactersPrinted = 0;

        private int MaxRowDepth;
        private List<TextFormat> Formats;
        private StreamWriter sw;
        private List<String> out_lines;

        public PrettyPrinter(List<TextFormat> f, int d, StreamWriter Writer, List<String> out_lines)
        {
            Formats = f;
            MaxRowDepth = d;
            sw = Writer;
            this.out_lines = out_lines;
        }

        public void Write(string fileName, int recordCount, out string result, OutputStyle outputMode, bool headerRecord)
        {
            if (outputMode == OutputStyle.fixed_width)
            {
                WriteFixedWidth(fileName, recordCount, out result, headerRecord);
            }
            else if (outputMode == OutputStyle.csv)
            {
                WriteCSV(fileName, recordCount, out result, headerRecord);
            }
            else
                throw new ArgumentException("Invalid output mode!");
        }

        public void WriteCSV(string fileName, int recordCount, out string result, bool headerRecord)
        {
            result = "";
            
            if (headerRecord)
            {
                string t = "";
                string[] s;
                string f;

                for (int i = 0; i < Formats.Count; i++)
                {
                    if (i != 0)
                        t += ",";

                    // split on dots and select the last section (simple name of the field)
                    f = Formats[i].FieldExpression;

                    if (f.StartsWith("="))                      // Calculated expression
                    {
                        t += f.Substring(1);
                    }
                    else if (f.StartsWith("Literal:"))
                    {
                        t += f.Substring(8).Trim();
                    }
                    else
                    {
                        s = f.Split(new char[] { '.' });
                        t += s[s.Length - 1].Trim();            // last element of s
                    }
                }

                sw.WriteLine(t);
                out_lines.Add(t);
            }

            string temp = "";

            // TESTING ONLY!!!!!
            //sw.WriteLine("TESTING!12345");
            // TESTING ONLY!!!!!

            for (int k = 0; k < recordCount; k++)
            {
                temp = "";

                for (int i = 0; i < Formats.Count; i++)
                {
                    if (i != 0)
                        temp += ",";

                    temp += Formats[i].SimpleFormat(k);
                }

                sw.WriteLine(temp);
            }

            sw.Flush();
        }

        public void WriteFixedWidth(string fileName, int recordCount, out string result, bool headerRecord)
        {
            if (headerRecord)
                throw new InvalidOperationException("Header Record output is currently unsupported for Fixed Width reports");

            result = "";

            string tempBuild = "";

            int rowStart;
            int rowEnd;

            for (int k = 0; k < recordCount; k++)
            {
                rowStart = 0;
                rowEnd = 0;

                for (int i = 1; i <= MaxRowDepth; i++)
                {
                    tempBuild = "";
                    rowEnd = GetRowBound(rowStart, i);

                    for (int j = rowStart; j < rowEnd; j++)
                    {
                        if (j == rowEnd - 1)
                        {
                            tempBuild += Formats[j].FormatRecord(-1, k);
                        }
                        else
                        {
                            tempBuild += Formats[j].FormatRecord(Formats[j + 1].Column - Formats[j].Column, k);
                        }
                    }

                    result += tempBuild + "\n";

                    ProcessNewlines(sw, tempBuild);
                    rowStart = rowEnd;
                }
            }

            sw.Flush();
        }

        public void ProcessNewlines(StreamWriter sw, string append)
        {
            int searchIndex = -1;
            int oldIndex = -1;

            string line;

            do
            {
                searchIndex = append.IndexOf('\n', searchIndex + 1);

                if (searchIndex != -1)
                {
                    line = append.Substring(oldIndex + 1, searchIndex - (oldIndex + 1));
                    sw.WriteLine(line);
                    out_lines.Add(line);
                    linesPrinted++;
                    charactersPrinted += searchIndex - (oldIndex + 1);
                }
                else
                {
                    line = append.Substring(oldIndex + 1);
                    sw.WriteLine(line);
                    out_lines.Add(line);
                    linesPrinted++;
                    charactersPrinted += append.Length - (oldIndex + 1);
                }

                oldIndex = searchIndex;
            } while (searchIndex != -1);
        }

        public int GetRowBound(int start, int rowNumber)
        {
            while ((start != Formats.Count) && Formats[start].Row == rowNumber)
                start++;

            return start;
        }
    }
}
