using System.ServiceModel.Syndication;
using System.Xml;
using JobSeeker.Application.Interfaces;

namespace JobSeeker.Infrastructure.Feed
{
    public class RssParser : IRssParser
    {
        public void ParseRss()
        {
            SyndicationFeed feed = null;

            try
            {
                using (var reader = XmlReader.Create("https://visualstudiomagazine.com/rss-feeds/news.aspx"))
                {
                    feed = SyndicationFeed.Load(reader);
                }
            }
            catch { } // TODO: Deal with unavailable resource.

            if (feed != null)
            {
                foreach (var element in feed.Items)
                {
                    Console.WriteLine($"Title: {element.Title.Text}");
                    Console.WriteLine($"Summary: {element.Summary.Text}");
                }
            }
        }
    }
}
