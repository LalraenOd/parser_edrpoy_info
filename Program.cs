using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace parser_edrpoy_info
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Default;
            Console.OutputEncoding = Encoding.Default;
            var sw = new Stopwatch();
            List<string> listEdrpou = new List<string>();

            Console.Write("Enter city: ");
            string city = Console.ReadLine();
            Console.Write("Enter youcontrol link to parse:");
            string linkSearch = Console.ReadLine();
            Console.WriteLine();
            //Getting EDRPOU's list from youcontrol
            sw.Start();
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                Console.WriteLine("Getting EDRPOY's...");
                Regex regexEdrpouCount = new Regex("</span> из <span class=\"text-green\">(?<result>\\d+)</span> найденных</div>");
                string htmlSearch = client.DownloadString(linkSearch);
                string matchedrpoyCount = regexEdrpouCount.Match(htmlSearch).Groups[1].Value.ToString();
                double pagesCount = Int32.Parse(matchedrpoyCount) / 20.0;
                for (int page = 1; page < Math.Ceiling(pagesCount) + 1; page++)
                {
                    string htmlPageSearch = client.DownloadString(linkSearch + "&page=" + page);
                    Regex regexEdrpoy = new Regex(pattern: "<a href=\"/ru/catalog/company_details/(?<result>.+)/\">");
                    MatchCollection matchCollectionEdrpoy = regexEdrpoy.Matches(htmlPageSearch);
                    Console.WriteLine("Found {0} EDRPOU's on page {1}", matchCollectionEdrpoy.Count.ToString(), page);
                    for (int matchCollectionCount = 0; matchCollectionCount < matchCollectionEdrpoy.Count - 1; matchCollectionCount++)
                    {
                        listEdrpou.Add(matchCollectionEdrpoy[matchCollectionCount].Groups["result"].Value.ToString());
                    }
                }
            }
            Console.WriteLine("Getting EDRPOU's list: DONE\n", Console.ForegroundColor = ConsoleColor.Green);

            //Getting details from http://edr.data-gov-ua.org/api
            Console.WriteLine("Getting details...", Console.ForegroundColor = ConsoleColor.Gray);
            EdrpouDetailParser(listEdrpou, city);
            sw.Stop();
            TimeSpan elapsedTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            Console.WriteLine("Done. Elapsed time: {0} seconds ", elapsedTime.ToString(), Console.ForegroundColor = ConsoleColor.Green);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Get's detail EDRPOU information from gttp://edr.data-gov-ua.org/api
        /// </summary>
        /// <param name="listEdrpou"> List of EDRPOU's to parse</param>
        /// <param name="fileName">Output file name *.txt</param>
        private static void EdrpouDetailParser(List<string> listEdrpou, string fileName)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                for (int listItemCount = 0; listItemCount < listEdrpou.Count - 1; listItemCount++)
                {
                    var edrpou = listEdrpou[listItemCount];
                    var linkEdrpou = "http://edr.data-gov-ua.org/api/companies?where={\"edrpou\":{\"contains\":\"" + edrpou + "\"}}";
                    string pageEdrpou = client.DownloadString(linkEdrpou);
                    Regex regexOfName = new Regex("\"officialName\":(?<result>.+)\",\"address");
                    Regex regexOccupation = new Regex("\"occupation\":\"(?<result>.+)\",\"status");
                    Regex regexStatus = new Regex("\"status\":\"(?<result>.+)\",\"id");
                    Match matchName = regexOfName.Match(pageEdrpou);
                    Match matchOcc = regexOccupation.Match(pageEdrpou);
                    Match matchStatus = regexStatus.Match(pageEdrpou);
                    string resultName = matchName.Groups[1].Value;
                    string resultOcc = matchOcc.Groups[1].Value;
                    string resultStatus = matchStatus.Groups[1].Value;
                    string result = (listItemCount + 1) + "\t" + edrpou + "\t" + resultName + "\t" + resultOcc + "\t" + resultStatus;
                    File.AppendAllText(fileName + ".txt", result + "\n");
                    if (listItemCount % 5 == 0)
                    {
                        Console.WriteLine("Done {0} EDRPOU's.", listItemCount, Console.ForegroundColor = ConsoleColor.Yellow);
                    }
                }
            }
        }
    }
}