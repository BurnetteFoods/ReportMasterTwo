using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.Common;
using System.Windows.Forms;

namespace ReportMasterTwo
{
    public class ParamInput
    {
        public bool DateRangeEnabled;
        public bool QuarterSelectorEnabled;
        public DateTime LowerDate;
        public DateTime UpperDate;
        public int QQuarter;
        public int QYear;

        public ParamInput()
        {
            DateRangeEnabled = false;
            QuarterSelectorEnabled = false;

            LowerDate = DateTime.Today;
            UpperDate = DateTime.Today;
            QQuarter = 0;
            QYear = 0;
        }

        public string eval(string paramName)
        {
            string p = paramName.ToUpper();
            string val;

            bool valid;

            switch (p)
            {
                case "QUARTER":
                    valid = QuarterSelectorEnabled;
                    val = QQuarter.ToString();
                    break;
                case "YEAR":
                    valid = QuarterSelectorEnabled;
                    val = QYear.ToString();
                    break;
                case "STARTDATE":
                    valid = DateRangeEnabled;
                    val = LowerDate.ToString("MMddYYYY");
                    break;
                case "ENDDATE":
                    valid = DateRangeEnabled;
                    val = UpperDate.ToString("MMddYYYY");
                    break;
                default:
                    throw new Exception("Param type label is unknown");
            }

            if (!valid)
            {
                throw new Exception("There was an attempt to use a undeclared parameter.");
            }

            return val;
        }

        public void search(string line)
        {
            if (line.IndexOf("QUARTER?") != -1)
            {
                if (!QuarterSelectorEnabled && !DateRangeEnabled)
                {
                    QuarterSelectorEnabled = true;
                    SelectQuarter();
                }

                line = line.Replace("QUARTER?", "?");
            }
            else if (line.IndexOf("YEAR?") != -1)
            {
                line = line.Replace("YEAR?", "?");
            }
            else if (line.IndexOf("?") != -1 && !QuarterSelectorEnabled && !DateRangeEnabled)
            {
                DateRangeEnabled = true;
                SelectDataRange();
            }
        }

        private void SelectQuarter()
        {
            YearPicker yp = new YearPicker();
            DialogResult dr = yp.ShowDialog();

            if (dr == DialogResult.Cancel)
            {
                throw new Exception("Cancel on Quarter / Year selector.");
            }

            string q = yp.quarterBox.Text;
            string y = yp.yearBox.Text;

            int quarter;
            int year;

            if (!Int32.TryParse(q, out quarter))
            {
                throw new Exception("Invalid quarter entry data.");
            }
            else if (!Int32.TryParse(y, out year))
            {
                throw new Exception("Invalid year entry data.");
            }

            QQuarter = quarter;
            QYear = year;
        }

        private void SelectDataRange()
        {
            DateSelector ds = new DateSelector();
            DialogResult dr = ds.ShowDialog();

            if (dr == DialogResult.Cancel)
                throw new Exception("Cancel on date");

            LowerDate = DateTime.Parse(ds.StartDate);
            UpperDate = DateTime.Parse(ds.EndDate);
        }

        public void process(DbCommand comm, DatabaseMode DBMode)
        {
            if (DateRangeEnabled)
            {
                if (DBMode == DatabaseMode.providex)
                {
                    ((OdbcCommand)comm).Parameters.AddWithValue("p_dr", LowerDate);
                    ((OdbcCommand)comm).Parameters.AddWithValue("p_d2", UpperDate);
                }
                else if (DBMode == DatabaseMode.access)
                {
                    ((OleDbCommand)comm).Parameters.AddWithValue("p_dr", LowerDate);
                    ((OleDbCommand)comm).Parameters.AddWithValue("p_d2", UpperDate);
                }
                else if (DBMode == DatabaseMode.sql_server)
                {
                    ((SqlCommand)comm).Parameters.AddWithValue("p_dr", LowerDate);
                    ((SqlCommand)comm).Parameters.AddWithValue("p_d2", UpperDate);
                }
            }
            else if (QuarterSelectorEnabled)
            {
                if (DBMode == DatabaseMode.providex)
                {
                    ((OdbcCommand)comm).Parameters.AddWithValue("p_q", QQuarter);
                    ((OdbcCommand)comm).Parameters.AddWithValue("p_y", QYear);
                }
                else if (DBMode == DatabaseMode.access)
                {
                    ((OleDbCommand)comm).Parameters.AddWithValue("p_q", QQuarter);
                    ((OleDbCommand)comm).Parameters.AddWithValue("p_y", QYear);
                }
                else if (DBMode == DatabaseMode.sql_server)
                {
                    ((SqlCommand)comm).Parameters.AddWithValue("p_q", QQuarter);
                    ((SqlCommand)comm).Parameters.AddWithValue("p_y", QYear);
                }
            }
        }
    }
}
