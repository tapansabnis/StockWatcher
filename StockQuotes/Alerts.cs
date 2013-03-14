using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.Mail;

namespace StockQuotes
{
    public class Alerts
    {
        public ArrayList alertList;
        public int count = 0;
        private ArrayList popSent;
        private ArrayList mailSent;
        private ArrayList textSent;
        private string[] carrierList = {"","@txt.att.net", "@vtext.com", "@tmomail.net", "@messaging.sprintpcs.com", "@vmobl.com"};

        public Alerts()
        {
            alertList = new ArrayList();
            popSent = new ArrayList();
            mailSent = new ArrayList();
            textSent = new ArrayList();
        }

        public bool Contains(string stock)
        {
            if (!alertList.Equals(null))
            {
                foreach (Alert alert in alertList)
                {
                    if (alert.getTicker() == stock)
                        return true;
                }
            }
            return false;
        }

        public void CheckAlerts(string email, string text, int carrier)
        {
            foreach (Alert alert in alertList)
            {
                if (alert.getCurrentPrice() != 0.0)
                {
                    if (alert.getStrikeType())
                    {
                        if(alert.getCurrentPrice() < alert.getPrice()){
                            switch (alert.getAlertType())
                            {
                                case "Pop-up":
                                    if(!popSent.Contains(alert.getTicker())){
                                        System.Windows.Forms.MessageBox.Show(alert.getTicker() + " has dropped below $" + alert.getPrice());
                                        popSent.Add(alert.getTicker());
                                    }
                                    break;
                                case "Email":
                                    if (!mailSent.Contains(alert.getTicker()))
                                    {
                                        SendEmail(alert.getTicker(), alert.getPrice(), alert.getStrikeType(), email);
                                        mailSent.Add(alert.getTicker());
                                    }
                                    break;
                                case "Text":
                                    if (!textSent.Contains(alert.getTicker()))
                                    {
                                        if (carrier != 0)
                                        {
                                            SendText(alert.getTicker(), alert.getPrice(), alert.getStrikeType(), text, carrier);
                                            textSent.Add(alert.getTicker());
                                        }
                                    }
                                    break;
                            }

                            }
                    }
                    else
                    {
                        if (alert.getCurrentPrice() > alert.getPrice())
                        {
                            switch (alert.getAlertType())
                            {
                                case "Pop-up":
                                    if (!popSent.Contains(alert.getTicker()))
                                    {
                                        System.Windows.Forms.MessageBox.Show(alert.getTicker() + " has risen above $" + alert.getPrice());
                                        popSent.Add(alert.getTicker());
                                    }
                                    break;
                                case "Email":
                                    if (!mailSent.Contains(alert.getTicker()))
                                    {
                                        SendEmail(alert.getTicker(), alert.getPrice(), alert.getStrikeType(), email);
                                        mailSent.Add(alert.getTicker());
                                    }
                                    break;
                                case "Text":
                                    if (!textSent.Contains(alert.getTicker()))
                                    {
                                        if (carrier != 0)
                                        {
                                            SendText(alert.getTicker(), alert.getPrice(), alert.getStrikeType(), text, carrier);
                                            textSent.Add(alert.getTicker());
                                        }
                                    }
                                    break;
                            }

                        }
                    }
                }
            }
        }

        private void SendText(string ticker, double price, bool type, string text, int carrier)
        {
            var fromAddress = new MailAddress("<insert email>", "Stock Alerts");
            
            
            var toAddress = new MailAddress(text+carrierList[carrier], "You");
            const string fromPassword = "<insert password>";
            const string subject = "Stock Alert";
            string body = "";
            if (type)
                body = ticker + " has dropped below $" + price;
            else
                body = ticker + " has risen above $" + price;

            var smtp = new SmtpClient
            {
                Host = "<insert host>",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }            
        }

        public void SendEmail(string ticker, double price, bool type, string email)
        {

            var fromAddress = new MailAddress("<insert email>", "Stock Alerts");
            var toAddress = new MailAddress(email, "You");
            const string fromPassword = "<insert password>";
            const string subject = "Stock Alert";
            string body = "";
            if(type)
                body = ticker + " has dropped below $" + price;
            else
                body = ticker + " has risen above $" + price;

            var smtp = new SmtpClient
            {
                Host = "<insert host>",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        public bool AddAlert(Alert alert)
        {
            try
            {
                alertList.Add(alert);
                count++;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool RemAlert(Alert alert)
        {
            try
            {
                alertList.Remove(alert);
                count--;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public Alert GetAlert(int index)
        {
            return (Alert)alertList[index];
        }

        public void SetPrice(string ticker, double currentPrice)
        {
            foreach (Alert alert in alertList)
            {
                if (alert.getTicker() == ticker)
                    alert.setPrice(currentPrice);                    
            }
        }
        //public string 
        public ArrayList AsList()
        {
            ArrayList temp = alertList;
            return temp;
        }
    }

    public class Alert
    {
        private string ticker;
        private double sPrice;
        private double cPrice;
        private string aType;
        private bool sTypeBelow;

        public Alert(string ticker, double price, double currentPrice, string type, string sType)
        {
            this.ticker = ticker;
            this.sPrice = price;
            this.cPrice = currentPrice;
            this.aType = type;
            if (sType == "Below")
                sTypeBelow = true;
            else
                sTypeBelow = false;
        }

        public string getTicker()
        {
            return this.ticker;
        }

        public double getPrice()
        {
            return this.sPrice;
        }

        public string getAlertType()
        {
            return this.aType;
        }
        public bool getStrikeType()
        {
            return this.sTypeBelow;
        }
        public double getCurrentPrice()
        {
            return this.cPrice;
        }
        public void setPrice(double currentPrice)
        {
            this.cPrice = currentPrice;
        }
    }
}
