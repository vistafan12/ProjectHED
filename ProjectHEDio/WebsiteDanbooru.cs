﻿using System;
using System.Threading;
using System.Text.RegularExpressions;
using MetroFramework.Controls;

namespace ProjectHEDio
{
    class WebsiteDanbooru : Website
    {
        public WebsiteDanbooru(MetroPanel sourcePanel) : base(sourcePanel)
        {

        }

        public override void InitializeScrape(string[] arguments = null, int limit = 1, bool limitByImages = false)
        {
            ScrapeThread = new Thread(() => Scrape(arguments, limit, limitByImages));
            ScrapeThread.IsBackground = true;
            ScrapeThread.Start();
        }

        protected override int GetMaxPages(string[] arguments = null)
        {
            string source = GetSource(FormatURL(arguments));
            if (source.Contains("There is currently no wiki page for the tag") || source.Contains("Nobody here but us chickens!"))
            {
                return 0;
            }
            if (!source.Contains("<a rel=\"next\""))
            {
                return 1;
            }

            // \.{1,3}<\/li><li><a href="[^>]+>(?<LastPage>\d+)
            string pattern = "\\.{1,3}<\\/li><li><a href=\"[^>]+>(?<LastPage>\\d+)";
            string match = Regex.Match(source, pattern).Groups["LastPage"].Value;
            int result = -1;
            if (int.TryParse(match, out result))
            {
                return result;
            }
            return -1;
        }

        protected override string FormatURL(string[] arguments = null, int pageNumber = 1)
        {
            // No API usage because it requires a key :c
            return string.Format("http://danbooru.donmai.us/posts?ms=1&page={0}&tags={1}", pageNumber, GetTagList(arguments));
        }

        protected override void Scrape(string[] arguments = null, int limit = 1, bool limitByImages = false)
        {
            int pages = GetMaxPages(arguments);
            if (pages < 1)
            {
                return;
            }
            if (!limitByImages && limit < pages)
            {
                pages = limit;
            }
            ulong totalFound = 0;
            for (int i = 1; i <= pages; i++)
            {
                string source = GetSource(FormatURL(arguments, i));
                // Images can be retrieved from article elements.
                // data-file-url="(?<Link>[^"]+)"\s+(?:data-large-file-url="(?<LinkLarge>[^"]+)")?
                string pattern = "data-file-url=\"(?<Link>[^\"]+)\"\\s+(?:data-large-file-url=\"(?<LinkLarge>[^\"]+)\")?";
                MatchCollection mc = Regex.Matches(source, pattern);
                foreach (Match m in mc)
                {
                    string link = "";
                    if (!string.IsNullOrWhiteSpace(m.Groups["LinkLarge"].Value))
                    {
                        link = m.Groups["LinkLarge"].Value;
                    }
                    else
                    {
                        link = m.Groups["Link"].Value;
                    }

                    if (limitByImages)
                    {
                        if ((int)totalFound < limit)
                        {
                            AddToLinks("http://danbooru.donmai.us" + link);
                            totalFound++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        AddToLinks("http://danbooru.donmai.us" + link);
                        totalFound++;
                    }
                }
                LogHelper.Log(string.Format("PAGE: Found {0} matches from page {1} of this {2} object.", mc.Count, i, this.ToString()));
                if (limitByImages && (int)totalFound >= limit)
                {
                    break;
                }
                // totalFound = totalFound + (uint)mc.Count;
            }
            LogHelper.Log(string.Format("SCRAPE: Found {0} total matches from this {1} object.", totalFound, this.ToString()));
        }
    }
}
