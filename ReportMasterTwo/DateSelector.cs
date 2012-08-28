using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ReportMasterTwo
{
    public partial class DateSelector : Form
    {
        public string StartDate;
        public string EndDate;

        public DateSelector()
        {
            InitializeComponent();
            StartDate = null;
            EndDate = null;
            monthCalendar1_DateChanged(null, null);
            monthCalendar2_DateChanged(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartDate = textBox1.Text;
            EndDate = textBox2.Text;

            DateTime dt;

            if (!DateTime.TryParse(StartDate, out dt))
            {
                textBox1.Text = "";
                textBox2.Text = "";
                label5.Visible = true;
            }
            else if (!DateTime.TryParse(EndDate, out dt))
            {
                textBox1.Text = "";
                textBox2.Text = "";
                label5.Visible = true;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string ts = textBox1.Text;
            int ind;

            ind = ts.IndexOf('/');

            if (ind == -1)
                return;

            ind = ts.IndexOf('/', ind + 1);

            if (ind == -1)
                return;

            if (ts.Length - ind != 5)
                return;

            DateTime dt;

            bool p = DateTime.TryParse(textBox1.Text, out dt);

            if(p)
            {
                monthCalendar1.SetDate(dt);
                label5.Visible = false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string ts = textBox2.Text;
            int ind;

            ind = ts.IndexOf('/');

            if (ind == -1)
                return;

            ind = ts.IndexOf('/', ind + 1);

            if (ind == -1)
                return;

            if (ts.Length - ind != 5)
                return;

            DateTime dt;

            bool p = DateTime.TryParse(textBox2.Text, out dt);

            if(p)
            {
                monthCalendar2.SetDate(dt);
                label5.Visible = false;
            }
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            textBox1.Text = monthCalendar1.SelectionStart.ToShortDateString();
        }

        private void monthCalendar2_DateChanged(object sender, DateRangeEventArgs e)
        {
            textBox2.Text = monthCalendar2.SelectionStart.ToShortDateString();
        }


    }
}