using System;
using System.Collections.Generic;
using System.Text;

namespace ReportMasterTwo
{
    public class TextFormat
    {
        public string FieldExpression;
        public string Position;
        public int Row;
        public int Column;
        public string Format;
        public List<object> Results;

        public TextFormat(string line)
        {
            int rbraceRectan;
            int lbraceSquig;
            int rbraceSquig;

            if (line[0] == '[')
            {
                rbraceRectan = line.LastIndexOf(']');

                if (rbraceRectan == -1)
                    throw new ArgumentException("File contains unmatched left brace [");

                FieldExpression = line.Substring(1, rbraceRectan - 1);
            }
            else
            {
                FieldExpression = line.Split(null)[0];
                rbraceRectan = FieldExpression.Length - 1;
            }

            lbraceSquig = line.LastIndexOf('{');
            rbraceSquig = line.LastIndexOf('}');

            if (lbraceSquig != -1)
            {
                if (rbraceSquig == -1)
                    throw new ArgumentException("File contains unmatched left brace {");

                Format = line.Substring(lbraceSquig, rbraceSquig - lbraceSquig + 1);
            }
            else
                throw new ArgumentException("Some formatting information is missing");

            Position = line.Substring(rbraceRectan + 1, (lbraceSquig - 1) - (rbraceRectan + 1) + 1);

            string[] positions = Position.Split(new char[] { '\\', '/' });

            if (positions.Length == 2)
            {
                Row = Convert.ToInt32(positions[0]);
                Column = Convert.ToInt32(positions[1]);
            }
            else if (positions.Length == 1)
            {
                Row = 1;
                Column = Convert.ToInt32(positions[0]);
            }
            else
                throw new ArgumentException("Incorrect Position Format");
        }

        public void AddResults(List<object> r)
        {
            Results = r;
        }

        public string SimpleFormat(int resultsIndex)
        {
            string temp = string.Format(Format, Results[resultsIndex]);

            // CSV must remove extraneous ,s
            temp = temp.Replace(',', ' ');
            temp = temp.TrimStart(null);
            temp = temp.TrimEnd(null);

            return temp;
        }

        public string FormatRecord(int resultsIndex)
        {
            return(FormatRecord(-1, resultsIndex));
        }

        public string FormatRecord(int mandatoryLength, int resultsIndex)
        {
            object temp = Results[resultsIndex];

            string tempS = string.Format(Format, temp);

            if (mandatoryLength == -1)
                return (tempS);
            else if (tempS.Length > mandatoryLength)
            {
                tempS = tempS.Substring(0, mandatoryLength);
                return (tempS);
            }
            else
                return (string.Format("{0, " + (-mandatoryLength) + "}", tempS));
        }

        public static int Comparer(TextFormat f1, TextFormat f2)
        {
            if (f1.Row < f2.Row)
            {
                return -1;
            }
            else if (f1.Row > f2.Row)
            {
                return 1;
            }
            else
            {
                if (f1.Column < f2.Column)
                {
                    return -1;
                }
                else if (f1.Column > f2.Column)
                {
                    return 1;
                }
                else
                    return 0;
            }
        }
    }
}
