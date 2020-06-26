using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace parser_edrpoy_info
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter city: ");
            string city = Console.ReadLine();
            while (true)
            {
                Console.InputEncoding = Encoding.Default;
                Console.OutputEncoding = Encoding.Default;
                Console.Write("Enter link to parse:");
                string linkSearch = Console.ReadLine();
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8; 
                    Console.WriteLine("Getting EDRPOY's...");
                    Regex regexEdrpoy = new Regex(pattern: "<a href=\"/ru/catalog/company_details/(?<result>.+)/\">");
                    Regex regexEdrpoyCount = new Regex(pattern: "<span class=\"text-green\">(?<result>.+)</span> найденных</div>");
                    string htmlSearch = client.DownloadString(linkSearch);
                    Match matchedrpoyCount = regexEdrpoyCount.Match(htmlSearch);
                    double pagesCount = matchedrpoyCount.Groups.Count / 10;
                    for (int page = 1; page < Math.Ceiling(pagesCount); page++)
                    {
                        string htmlPageSearch = client.DownloadString(linkSearch + "&page=" + page);
                        MatchCollection matchCollectionEdrpoy = regexEdrpoy.Matches(htmlPageSearch);
                        Console.WriteLine("Number of EDRPOY's on page {0}", matchCollectionEdrpoy.Count.ToString());
                        List<string> listEdrpoy = new List<string>();
                        for (int matchCollectionCount = 0; matchCollectionCount < matchCollectionEdrpoy.Count - 1; matchCollectionCount++)
                        {
                            listEdrpoy.Add(matchCollectionEdrpoy[matchCollectionCount].Groups["result"].Value.ToString());
                        } 
                    }
                }
                Thread.Sleep(300);
                Console.WriteLine("Done"); 
            }
        }
    }
}