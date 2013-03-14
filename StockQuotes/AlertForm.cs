using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StockQuotes
{
    public partial class AlertForm : Form
    {
        public AlertForm()
        {
            InitializeComponent();
        }

        private void popClrBtn_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
        }

        private void popSubBtn_Click(object sender, EventArgs e)
        {
            Alert alert = new Alert(textBox1.Text, double.Parse(textBox2.Text),0.0, comboBox1.Items[comboBox1.SelectedIndex].ToString(),comboBox2.Items[comboBox2.SelectedIndex].ToString());
            MainForm.addAlert(alert);
            this.Close();
        }
    }
}
