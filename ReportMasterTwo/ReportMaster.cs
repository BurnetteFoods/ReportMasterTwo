using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.Common;

namespace ReportMasterTwo
{
    public sealed class ReportMaster : IDisposable
    {
        private string ReportFileName;
        private string OutputFileName;
        private string ConnString;
        public List<TextFormat> FormatData;
        private List<string> FieldNames;
        private string SqlCommand;
        private Dictionary<string, List<object>> NameValueMap;
        private List<List<object>> FormatLinkedValues;
        private int RecordCount;
        private int DetailIndex;
        private ReportMaster DetailReport;
        private string DetailSubstitutionField;
        private List<object> DetailObjects;
        private int DetailRow;
        private int DetailColumn;
        public StreamReader Reader;
        public OutputStyle OperatingMode;
        public OutputTypeMode OutputMode;
        public DatabaseMode DBMode;
        public string email_address;
        public const string tempfile_base = @"";
        public ParsingMode ParseMode;
        public bool HeaderRecord;
        public ParamInput paramInput;
        public StreamWriter Writer;
        public List<String> out_lines;

        private string emailHost;

        public ReportMaster()
        {}

        public ReportMaster(string reportName, string outputName, string conn)
        {
            ReportFileName = reportName;
            
            if (outputName.StartsWith("mailto:"))
            {
                OutputFileName = tempfile_base + @"attach.txt";
                OutputMode = OutputTypeMode.send_to_email;
                email_address = outputName.Substring(7).Trim();
            }
            else
            {
                OutputFileName = outputName;
                OutputMode = OutputTypeMode.save_to_file;
            }

            ConnString = conn;
            FormatData = null;
            FieldNames = null;
            NameValueMap = null;
            SqlCommand = null;
            FormatLinkedValues = null;
            RecordCount = 0;
            DetailIndex = -1;
            OperatingMode = OutputStyle.fixed_width;
            ParseMode = ParsingMode.normal;
            DBMode = DatabaseMode.odbc;
            HeaderRecord = false;

            paramInput = new ParamInput();

            out_lines = new List<string>();

            emailHost = null;
        }

        public ReportMaster(string reportName, string outputName, string conn, string emailHost)
        {
            ReportFileName = reportName;

            if (outputName.StartsWith("mailto:"))
            {
                OutputFileName = tempfile_base + @"attach.txt";
                OutputMode = OutputTypeMode.send_to_email;
                email_address = outputName.Substring(7).Trim();
            }
            else
            {
                OutputFileName = outputName;
                OutputMode = OutputTypeMode.save_to_file;
            }

            ConnString = conn;
            FormatData = null;
            FieldNames = null;
            NameValueMap = null;
            SqlCommand = null;
            FormatLinkedValues = null;
            RecordCount = 0;
            DetailIndex = -1;
            OperatingMode = OutputStyle.fixed_width;
            ParseMode = ParsingMode.normal;
            DBMode = DatabaseMode.odbc;
            HeaderRecord = false;

            paramInput = new ParamInput();

            out_lines = new List<string>();

            this.emailHost = emailHost;
        }

        public ReportMaster(string reportName, string outputName, string conn, StreamReader s, StreamWriter w)
        {
            Reader = s;
            ReportFileName = reportName;
            OutputFileName = outputName;
            ConnString = conn;
            FormatData = null;
            FieldNames = null;
            NameValueMap = null;
            SqlCommand = null;
            FormatLinkedValues = null;
            RecordCount = 0;
            DetailIndex = -1;
            OperatingMode = OutputStyle.fixed_width;
            ParseMode = ParsingMode.normal;
            DBMode = DatabaseMode.odbc;
            HeaderRecord = false;

            out_lines = new List<string>();

            paramInput = new ParamInput();

            Writer = w;
        }

        public void Run()
        {
            ReadReportFile();
            
            RunDBQuery();
            ProcessExpressions();
            FormatResults();
            Reader.Close();
            Writer.Close();
        }

        public void FlushAll()
        {
            Reader.Close();
        }

        public void Dispose()
        {
            if(Reader != null)
            {
                Reader.Dispose();
            }

            if(Writer != null)
            {
                Writer.Dispose();
            }

            if(DetailReport != null)
            {
                DetailReport.Dispose();
            }
        }

        private void ReadReportFile()
        {
            if (Reader == null)
            {
                Reader = new StreamReader(ReportFileName);
            }

            ReadFormatData(Reader);
            ReadSqlData(Reader);
        }

        private void ProcessDatabaseResults(DbCommand comm)
        {
            paramInput.process(comm, DBMode);

            comm.CommandTimeout = 360;

            DbDataReader dr = comm.ExecuteReader();

            NameValueMap = new Dictionary<string, List<object>>();

            for (int i = 0; i < FieldNames.Count; i++)
            {
                NameValueMap.Add(FieldNames[i], new List<object>());
            }

            while (dr.Read())
            {
                for (int i = 0; i < FieldNames.Count; i++)
                {
                    NameValueMap[FieldNames[i]].Add(dr.GetValue(i));
                }

                RecordCount++;
            }

            if (DetailReport != null)
            {
                if (NameValueMap.ContainsKey(DetailSubstitutionField))
                {
                    DetailObjects = NameValueMap[DetailSubstitutionField];
                }
            }

            dr.Close();
        }

        private void RunDBQuery()
        {
            DbConnection conn = null;
            DbCommand comm = null;

            if (ConnString == "")
                throw new Exception("DB Connection Error!");

            if (DBMode == DatabaseMode.access)
            {
                conn = new OleDbConnection(ConnString);
                conn.Open();
                comm = new OleDbCommand(SqlCommand, (OleDbConnection)conn);
            }
            else if (DBMode == DatabaseMode.odbc)
            {
                conn = new OdbcConnection(ConnString);
                conn.Open();
                comm = new OdbcCommand(SqlCommand, (OdbcConnection)conn);
            }
            else if (DBMode == DatabaseMode.sql_server)
            {
                conn = new SqlConnection(ConnString);
                conn.Open();
                comm = new SqlCommand(SqlCommand, (SqlConnection)conn);
            }
            else
            {
                throw new InvalidOperationException("Invalid DBMode State");
            }

            ProcessDatabaseResults(comm);

            conn.Close();
        }

        private void ProcessExpressions()
        {
            List<ExpressionNode> nodes = new List<ExpressionNode>();

            for(int i = 0; i < FormatData.Count; i++)
            {
                nodes.Add(new ExpressionNode(FormatData[i].FieldExpression, paramInput));
            }

            FormatLinkedValues = new List<List<object>>(FormatData.Count);

            for (int i = 0; i < FormatData.Count; i++)
            {
                if (i == DetailIndex)
                {
                    FormatLinkedValues.Add(ProcessDetail2());
                }

                FormatLinkedValues.Add(nodes[i].Eval(NameValueMap, RecordCount));
            }
        }

        private List<object> ProcessDetail2()
        {
            List<object> linkedValues = new List<object>();
            string results = "";
            string oldSqlCommand = DetailReport.SqlCommand;

            for (int i = 0; i < DetailObjects.Count; i++)
            {
                DetailReport.SqlCommand = DetailReport.SqlCommand.Replace("$D", DetailObjects[i].ToString());
                DetailReport.RunDBQuery();
                DetailReport.ProcessExpressions();
                results = DetailReport.FormatResults();
                linkedValues.Add(results);
                DetailReport.SqlCommand = oldSqlCommand;
                DetailReport.RecordCount = 0;
            }

            return linkedValues;
        }

        private string FormatResults() 
        {
            Writer = File.CreateText(OutputFileName);

            for (int i = 0; i < FormatData.Count; i++)
            {
                FormatData[i].AddResults(FormatLinkedValues[i]);
            }

            FormatData.Sort(TextFormat.Comparer);

            int maxDepth = FormatData[FormatData.Count - 1].Row;

            PrettyPrinter pp = new PrettyPrinter(FormatData, maxDepth, Writer, out_lines);
            string temp = "";
            pp.Write(OutputFileName, RecordCount, out temp, OperatingMode, HeaderRecord);
            Writer.Close();

            if (OutputMode == OutputTypeMode.send_to_email)
            {
                string tempFilename = ((OperatingMode == OutputStyle.fixed_width) ? tempfile_base + "attach.txt" : tempfile_base + "attach.csv");

                MailMessage mm = new MailMessage(email_address, email_address, "Report Results", "See attached report results.");
                mm.Attachments.Add(new Attachment(tempFilename));


                SmtpClient client = new SmtpClient(emailHost);
                client.Send(mm);
            }

            return temp;
        }

        private void ProcessDetail(string line, StreamReader sr, int lineNum, List<TextFormat> FormatData)
        {
            DetailIndex = lineNum;

            if (sr.ReadLine() != "#Format Start#")
            {
                throw new ArgumentException("Invalid Formatting File");
            }
            
            DetailReport = new ReportMaster(ReportFileName, OutputFileName, ConnString, sr, Writer);

            // carry the params into the detail
            DetailReport.paramInput = paramInput;
            
            DetailReport.ReadReportFile();

            line = line.Substring(17);
            TextFormat t = new TextFormat(line);
            DetailSubstitutionField = t.FieldExpression.ToUpper();
            DetailRow = t.Row;
            DetailColumn = t.Column;
            FormatData.Add(t);
        }

        private void ReadFormatData(StreamReader sr)
        {
            int lineCounter = 0;
            FormatData = new List<TextFormat>();
            string line;

            while ((line = sr.ReadLine()) != "#Sql Start#")
            {
                ProcessLine(line, lineCounter, FormatData, sr);
            }
        }

        public void ProcessLine(string line, int lineNum, List<TextFormat> data, StreamReader sr)
        {
            if (line.StartsWith("#Detail Start"))
            {
                ProcessDetail(line, sr, lineNum, FormatData);
                lineNum++;
            }
            else if (line.StartsWith("#Header Record#"))
            {
                HeaderRecord = true;
            }
            else if (line.StartsWith("#Format Start#"))
            {
                if (ParseMode == ParsingMode.config)
                {
                    ParseMode = ParsingMode.normal;
                }

                OperatingMode = OutputStyle.fixed_width;

                if (OutputMode == OutputTypeMode.send_to_email)
                    OutputFileName = tempfile_base + "attach.txt";
                return;
            }
            else if (line.StartsWith("#CSV Start#"))
            {
                if (ParseMode == ParsingMode.config)
                {
                    ParseMode = ParsingMode.normal;
                }

                OperatingMode = OutputStyle.csv;

                if (OutputMode == OutputTypeMode.send_to_email)
                    OutputFileName = tempfile_base + "attach.csv";

                return;
            }
            else if (line.StartsWith("#Config Start#"))
            {
                ParseMode = ParsingMode.config;
            }
            else if (ParseMode == ParsingMode.config)
            {
                ProcessConfigLine(line);
            }
            else
            {
                FormatData.Add(new TextFormat(line));
                lineNum++;
            }
        }

        public void ProcessConfigLine(string line)
        {
            int valueIndex = line.IndexOf(':') + 1;
            string value = line.Substring(valueIndex).Trim();

            if (line.IndexOf("Connection-String") != -1)
            {
                ConnString = value;
            }
            else if (line.IndexOf("Connection-Type") != -1)
            {
                if (value.Equals("SqlServer"))
                {
                    DBMode = DatabaseMode.sql_server;
                }
                else if (value.Equals("Access"))
                {
                    DBMode = DatabaseMode.access;
                }
                else if (value.Equals("Providex"))
                {
                    DBMode = DatabaseMode.odbc;
                }
            }
        }

        public void ProcessLine(string line, int lineNum, List<TextFormat> data)
        {
            FormatData.Add(new TextFormat(line));
            lineNum++;
        }

        private void ReadSqlData(StreamReader sr)
        {
            string line;

            string sqlTemp = "";

            while ((line = sr.ReadLine()) != "#Report End#")
            {
                line = paramInput.search(line);


                sqlTemp += line;
            }

            SqlCommand = sqlTemp;
            sqlTemp = sqlTemp.ToUpper();

            int selectIndex = sqlTemp.IndexOf("SELECT");

            sqlTemp = sqlTemp.Substring(selectIndex + 6, sqlTemp.IndexOf("FROM") - (selectIndex + 6));

            sqlTemp = sqlTemp.Replace(" ", "");

            List<string> names = SplitOnParenClear(sqlTemp);
                
            for (int i = 0; i < names.Count; i++)
            {
                names[i] = TrimWrappers(names[i]);
            }

            FieldNames = new List<string>(names);
        }

        private List<string> SplitOnParenClear(string val)
        {
            List<string> splits = new List<string>();
            int splitIndex = 0;

            string left;
            string right = val;

            while (true)
            {
                splitIndex = ExpressionNode.SplitOnParenClear(right, ',', ',');

                if (splitIndex == -1)
                {
                    splits.Add(right);
                    break;
                }
                else
                {
                    left = right.Substring(0, splitIndex);
                    right = right.Substring(splitIndex + 1);
                    splits.Add(left);
                }
            }

            return splits;
        }

        public string TrimWrappers(string inval)
        {
            inval = inval.Replace("[", "");
            inval = inval.Replace("\"", "");
            inval = inval.Replace("\n", "");
            inval = inval.Replace("]", "");
            inval = inval.Replace(" ", "");
            inval = inval.Replace("#", "");

            return inval;
        }
    }
}
