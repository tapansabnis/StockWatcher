using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using StockQuotes.Properties;
using System.Xml;

namespace StockQuotes
{
    public partial class MainForm : Form
    {
        string[][] alertPrices = new string[100][];
        string[] paramList, chartUrlList;
        string parameters = "&f=sl1c1n";
        string refreshParams = "";
        int count = 0;
        ListViewItem[] quoteUpdates;
        BackgroundWorker worker = new BackgroundWorker();
        BackgroundWorker alertWorker = new BackgroundWorker();
        BackgroundWorker chartWorker = new BackgroundWorker();
        string analysisStock = "GOOG";
        public static Alerts alertCollection;
        string email = "";
        string text = "";
        int carrier = 0;

        

        public MainForm()
        {
            InitializeComponent();
            paramList = new string[] { "k", "j", "b2", "b3", "a2", "h", "g", "k2","r","o","p","x","m" };
            chartUrlList = new string[]{
            @"http://ichart.finance.yahoo.com/b?s=", @"http://ichart.finance.yahoo.com/w?s=", 
            @"http://chart.finance.yahoo.com/c/3m/", @"http://chart.finance.yahoo.com/c/6m/", 
            @"http://chart.finance.yahoo.com/c/1y/", @"http://chart.finance.yahoo.com/c/2y/", 
            @"http://chart.finance.yahoo.com/c/5y/", @"http://chart.finance.yahoo.com/c/my/"};

            LoadSettings();
            
            InitiateWorkers();

            HtmlDocument doc = (HtmlDocument)indexBrowser.Document;
            if(doc != null)
            doc.Body.Name =  "";
            updateVars();
            indexBrowser.Navigate(@"http://apps.cnbc.com/view.asp?YYY330_XPDfu5vdhz9HyCRjr3G70R0A9hRO7fWtFsoR8c7jlUv40AuQQ+bZ2e2tRbDtPKi0jNSra6i07nSOl37m3JhZcF6UFhBqkWisrbBSsTWFCmq/HD2BL2xT3A==&uid=markets/summary");
            axWebBrowser6.Navigate(@"http://www.cnbc.com/id/19789731/device/rss/rss.xml");

            setRSSLabels();
            
        }

        private void setRSSLabels()
        {
            RSSReader rss = new RSSReader();
            List<RSSNews> rssList = rss.Read(@"http://www.cnbc.com/id/100003114/device/rss/rss.html");

            int count = 0;
            foreach (RSSNews newsLink in rssList)
            {
                if (count < groupBox2.Controls.Count)
                {
                    LinkLabel tempLabel = (LinkLabel)groupBox2.Controls[count];
                    tempLabel.Text = newsLink.Title;
                    LinkLabel.Link tempLink = new LinkLabel.Link();
                    tempLink.LinkData = newsLink.Link;
                    tempLabel.Links.Add(tempLink);
                    tempLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
                    tempLabel.AutoSize = true;
                    // Position and size the control on the form.
                    groupBox2.Controls[count].Visible = true;                    
                    tempLabel.BringToFront();

                }
                count++;                
            }
        }

        private void linkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = (LinkLabel)sender;
            // Determine which link was clicked within the LinkLabel.
            linkLabel.Links[linkLabel.Links.IndexOf(e.Link)].Visited = true;
            // Display the appropriate link based on the value of the 
            // LinkData property of the Link object.
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void LoadSettings()
        {
            //LOAD SETTINGS
            //Company List
            if (Settings.Default.CList != null)
            {
                listBox1.Items.Clear();
                foreach (string company in Settings.Default.CList)
                {
                    listBox1.Items.Add(company);
                }
            }
            //Parameter list
            if (Settings.Default.Plist != null)
            {
                foreach (int index in Settings.Default.Plist)
                {
                    checkedListBox1.SetItemChecked(index, true);
                }
            }
            else
            {

                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
            }
            //Refresh Rate
            if (Settings.Default.refresh != 55)
                comboBox2.SelectedIndex = Settings.Default.refresh;
            else
                comboBox2.SelectedIndex = 1;
            this.count = checkedListBox1.SelectedIndices.Count;

            //Email
            if (Settings.Default.Email != null)
                txtEmail.Text = Settings.Default.Email;

            //Phone number
            if (Settings.Default.Text != null)
                txtPhone.Text = Settings.Default.Text;

            //Carrier
            comboBox2.SelectedIndex = Settings.Default.Carrier;

            alertCollection = new Alerts();
            if (Settings.Default.Alerts != null)
            {
                alertCollection.alertList = Settings.Default.Alerts;
                alertCollection.count = alertCollection.alertList.Count;
            }
            
        }

        private void InitiateWorkers()
        {
            chartWorker.DoWork += new DoWorkEventHandler(chartWorker_DoWork);
            chartWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(chartWorker_RunWorkerCompleted);
            chartWorker.WorkerSupportsCancellation = true;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.WorkerSupportsCancellation = true;


            alertWorker.DoWork += new DoWorkEventHandler(alertWorker_DoWork);
            alertWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(alertWorker_RunWorkerCompleted);
            alertWorker.WorkerSupportsCancellation = true;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; (i <= 10); i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    quoteUpdates = GetQuotes();
                }
            }
        }

        private void alertWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (alertCollection != null)
            {
                foreach (string[] tickerDouble in alertPrices)
                {
                    if (tickerDouble != null)
                    {
                        if (alertCollection.Contains(tickerDouble[0]))
                            alertCollection.SetPrice(tickerDouble[0], double.Parse(tickerDouble[1]));
                    }
                }
                if(alertCollection.count > 0)
                alertCollection.CheckAlerts(email, text, carrier);
            }
        }

        private void alertWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //alertWorker.RunWorkerAsync();
        }

        //Main Quote worker. Refreshes every 5 seconds by default.
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int selectedIndex = 0;
            if(listView1.SelectedIndices.Count > 0)
                selectedIndex = listView1.SelectedIndices[0];
            listView1.BeginUpdate();
            
            listView1.Items.Clear();
            listView1.Items.AddRange(quoteUpdates);
            listView1.Items[selectedIndex].Selected = true;
            listView1.EndUpdate();
            email = txtEmail.Text;
            text = txtPhone.Text;
            carrier = comboBox1.SelectedIndex;
            if(!alertWorker.IsBusy)
                alertWorker.RunWorkerAsync();
            
            if (!worker.IsBusy)
            {

                int index = comboBox2.SelectedIndex;
                int time = 5000;
                if (index >= 0)
                    time = int.Parse(comboBox2.SelectedItem.ToString()) * 1000;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    Thread.Sleep(time);
                    if (!worker.IsBusy)
                        Invoke(new Action(delegate { worker.RunWorkerAsync(); }));

                });

            }
        }

        //Refresh Button method. Updates Variables and refreshes values
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
            worker.Dispose();

            listView1.Items.Clear();
            if (checkedListBox1.InvokeRequired)
                checkedListBox1.BeginInvoke(new MethodInvoker(() => updateVars()));
            else
                updateVars();
            if (!worker.IsBusy)
                worker.RunWorkerAsync();

        }

        //Updates Quote variables 
        private void updateVars()
        {
            count = checkedListBox1.CheckedIndices.Count;
            parameters = getParameters();
            refreshParams = getRefreshParams();
            
        }

        private string getRefreshParams()
        {
            refreshParams = "&f=l1c1";
            string[] refreshList = {"a2", "b2", "c6", "k2"};
            foreach (int index in checkedListBox1.CheckedIndices)
            {
                if(refreshList.Contains(paramList[index]))
                    refreshParams += paramList[index];
            }
            
            return refreshParams;
        }

        //Main menthod for quote updates
        private ListViewItem[] GetQuotes()
        {

            int currentCount = count;
            ListViewItem[] items = new ListViewItem[listBox1.Items.Count];
            
            string companies = getCompanyString();
            string[] quotes = getQuoteString(companies, parameters);

            for (int i = 0; i < quotes.Length; i++)
            {
                string quote = quotes[i];
                string[] words = Regex.Split(quote, ",");

                ArrayList wordList = new ArrayList();
                wordList.AddRange(words);
                if(wordList.Count > currentCount + 4)
                    wordList.RemoveAt(1);                

                ListViewItem item = new ListViewItem(wordList[0].ToString().Replace("\"",""));
                for (int j = 1; j < currentCount + 4; j++)
                {
                    item.SubItems.Add(wordList[j].ToString().Replace("\"", "").Replace("NasdaqNM","Nasdaq").Replace("N/A -",""));
                }

                if (double.Parse(wordList[3].ToString()) > 0)
                    item.ForeColor = Color.LightGreen;
                else if (double.Parse(wordList[3].ToString()) < 0)
                    item.ForeColor = Color.OrangeRed;

                items[i] = item;
                alertPrices[i] = new string[] { wordList[1].ToString().Replace("\"", ""), wordList[2].ToString().Replace("\"", "") };
            }
            return items;
        }

        //Gets the quote string from Yahoo
        private string[] getQuoteString(string companies, string parameters)
        {
            string[] quotes = new string[listBox1.Items.Count];
            string yahooURL = @"http://download.finance.yahoo.com/d/quotes.csv?s=" +
                                  companies + parameters;
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(yahooURL);
            HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
            StreamReader strm = new StreamReader(webresp.GetResponseStream(), Encoding.ASCII);

            for (int i = 0; i < quotes.Length; i++)
            {
                quotes[i] = strm.ReadLine();
            }
            strm.Close();
            webreq.Abort();

            return quotes;
        }

        //Gets the selected parameters
        private string getParameters()
        {
            string parameters = "&f=nsl1c1";
            foreach (int index in checkedListBox1.CheckedIndices)
            {
                parameters += paramList[index];
            }

            if (listView1.InvokeRequired)
                listView1.BeginInvoke(new MethodInvoker(() => updateColumns()));
            else
                updateColumns();
            return parameters;

        }

        //Updates the columns when parameters are changed
        private void updateColumns()
        {
            listView1.Columns.Clear();
            listView1.Columns.Add("Company Name");
            listView1.Columns.Add("Symbol");
            listView1.Columns.Add("Last Price");
            listView1.Columns.Add("Yeild (real time)");

            foreach (int index in checkedListBox1.CheckedIndices)
            {
                listView1.Columns.Add(checkedListBox1.Items[index].ToString());
            }
            
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            
        }

        //Formats the companies string to be used in the query
        private string getCompanyString()
        {
            string comp = "";
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (i == listBox1.Items.Count)
                    comp += listBox1.Items[i];
                else
                    comp += listBox1.Items[i] + "+";
            }
            return comp;
        }

        //Add button event
        private void btnAdd_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
            textBox1.Text = "";
        }

        //Remove Button event
        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach(int index in listBox1.SelectedIndices)
            {
                listBox1.Items.RemoveAt(index);
            }
            
        }

        //Button event for chart update
        private void btnChartUpdate_Click(object sender, EventArgs e)
        {
            chartWorker.RunWorkerAsync();
        }

        //Do Work for chart worker
        private void chartWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker cworker = sender as BackgroundWorker;

            for (int i = 1; (i <= 10); i++)
            {
                if ((cworker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    axWebBrowser2.Navigate("http://apps.cnbc.com/view.asp?uid=stocks/charts&symbol=" + analysisStock);
                }
            }
        }

        private void chartWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            axWebBrowser2.Update();
        }
        
        //Triggers portfolio update
        private void portfolio_Click(object sender, EventArgs e)
        {
            ListView listView = new ListView();
            
            string tickerStr = "";
            
            int count = 0;

            if (tabControl2.SelectedIndex != tabControl2.TabPages.Count-1)
            {
                
                string fileName = "portfolio" + (tabControl2.SelectedIndex + 1).ToString() + ".txt";
                listView = (ListView)tabControl2.TabPages[tabControl2.SelectedIndex].Controls[0];

                NewPort portfolio = new NewPort();
                try
                {
                    StreamReader stream = new StreamReader(fileName);
                    stream.ReadLine();
                    while (!stream.EndOfStream)
                    {
                        string rawStock = stream.ReadLine();
                        if(rawStock != ""){
                        string[] splitStock = Regex.Split(rawStock, ",");
                        Stock stock = new Stock(splitStock[0], double.Parse(splitStock[1]), int.Parse(splitStock[2]));
                        portfolio.AddStock(stock);
                        tickerStr += splitStock[0] + "+";
                        count++;
                            }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                tickerStr.Remove(tickerStr.Length - 1);

                string[] portPrice = getPortQuotes(tickerStr, "&f=sl1c1n", count);
                portfolio.UpdatePrices(portPrice);

                ArrayList items = portfolio.GetList();
                updatePortList(items, listView);
            }

        }

        //Updates Listview for portfolio tab
        private void updatePortList(ArrayList items, ListView listView)
        {
            listView.BeginUpdate();
            listView.Items.Clear();
            foreach (Object item in items)
            {
                listView.Items.Add((ListViewItem)item);
            }
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.EndUpdate();
        }

 
        //Gets portfolio Quotes
        private string[] getPortQuotes(string companies, string parameters, int count)
        {
            string[] quotes = new string[count];
            string yahooURL = @"http://download.finance.yahoo.com/d/quotes.csv?s=" +
                                  companies + parameters;
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(yahooURL);
            HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
            StreamReader strm = new StreamReader(webresp.GetResponseStream(), Encoding.ASCII);

            int i = 0;
            while(!strm.EndOfStream){
            
                quotes[i] = strm.ReadLine();
                string[] words = Regex.Split(quotes[i], ",");
                quotes[i] = words[1];
                i++;
            }
            strm.Close();
            webreq.Abort();

            return quotes;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                return;
            }
            ListViewItem tempItem = (ListViewItem)this.listView1.Items[this.listView1.SelectedIndices[0]];
            analysisStock = tempItem.SubItems[1].Text;
            tabControl1.SelectTab(2);
            tabControl3.SelectTab(0);
            chartUpdater(analysisStock);
        }

        private void chartUpdater(string stock)
        {
           // if (!chartWorker.IsBusy)
                //chartWorker.RunWorkerAsync();
/*
            else
            {
                chartWorker.CancelAsync();
                chartWorker.Dispose();
                chartWorker.RunWorkerAsync();
            }
 * */
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedIndex != 1)
                {

                    worker.CancelAsync();
                }
            }
            catch (System.InvalidOperationException ex)
            {
            }
            if (tabControl1.SelectedIndex == 1)
            {
                if(!worker.IsBusy)
                    worker.RunWorkerAsync();
            }
            if (tabControl1.SelectedIndex == 2)
            {
                if (!chartWorker.IsBusy)
                    chartWorker.RunWorkerAsync();
            }

            if (tabControl1.SelectedIndex == 4)
                if (alertCollection != null)
                {
                    dataGridView1.Rows.Clear();
                    for (int i = 0; i < alertCollection.count; i++)
                    {


                        Alert alert = alertCollection.GetAlert(i);
                        DataGridViewRow tempRow = new DataGridViewRow();
                        DataGridViewCell cell1 = new DataGridViewTextBoxCell();
                        cell1.Value = alert.getTicker();
                        tempRow.Cells.Add(cell1);
                        DataGridViewCell cell2 = new DataGridViewTextBoxCell();
                        cell2.Value = alert.getPrice().ToString();
                        tempRow.Cells.Add(cell2);
                        DataGridViewCell cell3 = new DataGridViewTextBoxCell();
                        if (alert.getStrikeType())
                            cell3.Value = "Below";
                        else
                            cell3.Value = "Above";
                        tempRow.Cells.Add(cell3);
                        DataGridViewCell cell4 = new DataGridViewTextBoxCell();
                        cell4.Value = alert.getAlertType();
                        tempRow.Cells.Add(cell4);

                        dataGridView1.Rows.Add(tempRow);

                    }
                }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ArrayList compList = new ArrayList(listBox1.Items);
            Settings.Default.CList = compList;
            ArrayList parList = new ArrayList(checkedListBox1.CheckedIndices);
            ArrayList alertSaveList = alertCollection.AsList();
            Settings.Default.Plist = parList;
            Settings.Default.refresh = comboBox2.SelectedIndex;
            Settings.Default.Alerts = alertSaveList;
            Settings.Default.Email = txtEmail.Text;
            Settings.Default.Text = txtPhone.Text;
            Settings.Default.Carrier = comboBox2.SelectedIndex;
            Settings.Default.Save();
        }

        private void tabPage10_Click(object sender, EventArgs e)
        {

        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl3.SelectedIndex == 0)
            {
                if(!chartWorker.IsBusy)
                chartWorker.RunWorkerAsync();
            }
            if (tabControl3.SelectedIndex == 1)
            {
                axWebBrowser4.Navigate(@"http://apps.cnbc.com/view.asp?uid=stocks/financials&view=incomeStatement&symbol=" + analysisStock);
            }
            else if (tabControl3.SelectedIndex == 2)
            {
                axWebBrowser5.Navigate(@"http://apps.cnbc.com/view.asp?uid=stocks/earnings&annQtr=qtr&symbol=" + analysisStock);
            }

        }

        private void rightClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(this, new Point(e.X, e.Y));
                        
        }

        private void addAlertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string stock = this.listView1.Items[this.listView1.SelectedIndices[0]].SubItems[1].Text;
            AlertForm PopUp = new AlertForm();
            PopUp.Controls["textBox1"].Text = stock;
            PopUp.Show();
        }

        public static void addAlert(Alert alert)
        {
            alertCollection.AddAlert(alert);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox2.Text;
            ArrayList list = new ArrayList();
            string stock = "Ticker, Paid Price, Shares" + System.Environment.NewLine;

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (!row.IsNewRow )
                {
                    stock += row.Cells["Ticker"].Value.ToString() + "," + row.Cells["Paid"].Value.ToString() + "," + row.Cells["Shares"].Value.ToString() + System.Environment.NewLine;
                    list.Add(stock);
                }
            }

            tabControl2.TabPages.Insert(tabControl2.TabPages.Count, name);
            tabControl2.TabPages[tabControl2.TabPages.Count-1].Controls.Add(tabControl2.TabPages[1].Controls[0]);

            TabPage temp = tabControl2.TabPages[tabControl2.TabPages.Count-1];
            tabControl2.TabPages[tabControl2.TabPages.Count-1] = tabControl2.TabPages[tabControl2.TabPages.Count - 2];
            tabControl2.TabPages[tabControl2.TabPages.Count-2] = temp;
            tabControl2.SelectTab(tabControl2.TabPages[tabControl2.TabPages.Count - 1]);
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            string tempsd = appPath + "\\portfolio" + (tabControl2.TabPages.Count - 1).ToString() + ".txt";
            System.IO.StreamWriter file = new System.IO.StreamWriter(tempsd);
            file.WriteLine(stock);

            file.Close();
                        
        }
    }

    class ListViewNF : System.Windows.Forms.ListView
    {
        public ListViewNF()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }


}