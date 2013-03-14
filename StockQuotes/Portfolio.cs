using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace StockQuotes
{
    class Portfolio
    {
        public string[][] list;
        string[] prices;


        public Portfolio(string[][] list, string[] prices)
        {
            this.list = list;
            this.prices = prices;
        }

        public int getCount()
        {
            return prices.Length;
        }

        public void updatePort()
        {
            UpdateLastPrice();
            CalcMValue();
            CalcGain();
            CalcPGain();
            

        }

        private void CalcMValue()
        {
            for (int i = 0; i < list[0].Length; i++)
            {
                this.list[6][i] = ((double.Parse(list[1][i]) * double.Parse(list[5][i]))).ToString();
            }
        }

        private void CalcPGain()
        {
            for (int i = 0; i < list[0].Length; i++)
            {
                this.list[4][i] = ((double.Parse(list[3][i]) / double.Parse(list[6][i])) * 100).ToString();
            }
        }

        private void CalcGain()
        {
            
            for (int i = 0; i < list[0].Length; i++)
            {
                this.list[3][i] = ((double.Parse(list[1][i]) * double.Parse(list[5][i])) - (double.Parse(list[2][i]) * double.Parse(list[5][i]))).ToString();
            }
        }

        private void UpdateLastPrice()
        {
            for (int i = 0; i < prices.Length;i++ )
            {
                this.list[1][i] = prices[i];
            }
        }

    }
}
