using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;
using System.ServiceModel.Syndication;
using System.Xml;

namespace StockQuotes
{
    public class RSSReader
    {

        public List<RSSNews> Read(string url)
        {
            List<RSSNews> rssList = new List<RSSNews>();
            SyndicationFeed feed = SyndicationFeed.Load(XmlReader.Create(url));
            foreach (SyndicationItem item in feed.Items)
            {
                RSSNews tempLink = new RSSNews();
                tempLink.Title = item.Title.Text;
                tempLink.Link = item.Links[0].Uri;
                rssList.Add(tempLink);
            }

            return rssList;
        }
    }

    public class RSSNews
    {
        public string Title;
        public Uri Link;        
    }

}
 
