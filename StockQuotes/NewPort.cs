using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;

namespace StockQuotes
{
    class NewPort
    {
        private ArrayList stocks;
        
        public NewPort()
        {
            this.stocks = new ArrayList();
        }

        public void AddStock(Stock stock)
        {
            this.stocks.Add(stock);
        }

        public void UpdatePrices(string[] newPrices)
        {

            for (int i = 0; i < newPrices.Length; i++)
            {
                Stock stock = (Stock)stocks[i];
                stock.updatePrice(double.Parse(newPrices[i]));
                stock.updateValues();
            }
        }

        public ArrayList GetList()
        {
            double totalGain = 0;
            double totalPaid = 0;
            ArrayList items = new ArrayList();

            for (int i = 0; i < stocks.Count; i++)
            {
                Stock tempStock = (Stock)stocks[i];
                string[] tempList = tempStock.getInfoList();
                ListViewItem item = new ListViewItem(tempList[0]);
                totalGain += double.Parse(tempStock.getGain());
                totalPaid += double.Parse(tempStock.getPaid()) * double.Parse(tempStock.getShares());
                for (int j = 1; j < tempList.Length; j++)
                {
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, tempList[j]));
                }
                if (double.Parse(tempStock.getGain()) > 0)
                    item.ForeColor = Color.Green;
                else if (double.Parse(tempStock.getGain()) < 0)
                    item.ForeColor = Color.DarkRed;
                items.Add(item);
                
            }
            ListViewItem total = new ListViewItem("Total");
            total.SubItems.Add(new ListViewItem.ListViewSubItem(total, ""));
            total.SubItems.Add(new ListViewItem.ListViewSubItem(total, ""));
            total.SubItems.Add(new ListViewItem.ListViewSubItem(total, totalGain.ToString()));
            total.SubItems.Add(new ListViewItem.ListViewSubItem(total, (Math.Round((totalGain*100/totalPaid),2)).ToString()));
            if (totalGain > 0)
                total.ForeColor = Color.Green;
            else if (totalGain < 0)
                total.ForeColor = Color.DarkRed;
            items.Add(total);

            return items;
        }
        
    }

    class Stock{

        string ticker;
        double price;
        double paid;
        double marketValue;
        int shares;
        double gain;
        double gainPct;

        public Stock(string ticker, double paid, int shares)
        {
            this.ticker = ticker;
            this.paid = paid;
            this.shares = shares;

        }

        #region Getters
        public string getTicker()
        {
            return ticker;
        }
        public string getPaid()
        {
            return paid.ToString();
        }
        public string getPrice()
        {
            return price.ToString();
        }
        public string getMValue()
        {
            return marketValue.ToString();
        }
        public string getGain()
        {
            return gain.ToString();
        }
        public string getGainP()
        {
            return gainPct.ToString();
        }
        public string getShares()
        {
            return shares.ToString();
        }
        public string[] getInfoList()
        {
            string[] list = new string[]{getTicker(),getPrice(),getPaid(),getGain(),getGainP(),getShares(),getMValue()};
            return list;
            
        }
        #endregion

        public void updatePrice(double price)
        {
            this.price = price;
        }

        public void updateValues()
        {
            this.gain = Math.Round(((this.price - this.paid) * this.shares),2);
            this.marketValue = Math.Round((this.price * this.shares),2);
            this.gainPct = Math.Round(((this.gain / this.marketValue) * 100),2);
        }
    }
}
