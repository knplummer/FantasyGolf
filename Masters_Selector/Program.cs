using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Masters_Selector
{

    class MastersGolfer
    {
        public string Name { get; set; }
        public GolfRank CurrentRank { get; set; }

        public MastersGolfer () { }
        
        public MastersGolfer(string name)
        {
            Name = name;
        }
    }

    class GolfRank
    {
        public string Name { get; set; }
        public double ThisWeek { get; set; }
        public double LastWeek { get; set; }
        public double AveragePoints { get; set; }
        public double TotalPoints { get; set; }
        public double Score { get; set; }

        public GolfRank() { }

        public GolfRank(string name, double thisWeek, double lastWeek, double averagePoints, double totalPoints)
        {
            Name = name;
            ThisWeek = thisWeek;
            LastWeek = lastWeek;
            AveragePoints = averagePoints;
            TotalPoints = totalPoints;

            Score = (2.0 * AveragePoints) + (LastWeek / ThisWeek);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<GolfRank> golferRankings = new List<GolfRank>();
            List<MastersGolfer> mastersGolfers = new List<MastersGolfer>();

            string urlAddress = "http://www.owgr.com/ranking?pageNo=1&pageSize=300&country=All";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string rankData = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                rankData = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }

            int trCounter = 0;
            if (!string.IsNullOrEmpty((rankData)))
            {
                var lines = rankData.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim() == "<tr>")
                    {
                        string name = "";
                        double thisWeek = 0.0;
                        double lastWeek = 0.0;
                        double average = 0.0;
                        double total = 0.0;

                        if (trCounter > 0)
                        {
                            int tdCounter = 0;
                            int[] cells = { 0, 1, 4, 5 };
                            while (lines[i].Trim() != "</tr>")
                            {
                                switch (tdCounter)
                                {
                                    case 0:
                                        char[] week1 = lines[i + 1].Trim().ToCharArray();
                                        Array.Reverse(week1);
                                        int charCount1 = 0;
                                        List<char> week1CharList = new List<char>();
                                        foreach (char c in week1)
                                        {
                                            if (c == '>')
                                            {
                                                charCount1++;
                                            }
                                            if (charCount1 == 2)
                                            {
                                                break;
                                            }
                                            week1CharList.Add(c);
                                        }
                                        char[] week1Array = week1CharList.ToArray();
                                        Array.Reverse(week1Array);
                                        string w1String = new string(week1Array);
                                        thisWeek = Convert.ToDouble(w1String.Substring(0, w1String.Length - 5));
                                        break;
                                    case 1:
                                        char[] week2 = lines[i + 1].Trim().ToCharArray();
                                        Array.Reverse(week2);
                                        int charCount2 = 0;
                                        List<char> week2CharList = new List<char>();
                                        foreach (char c in week2)
                                        {
                                            if (c == '>')
                                            {
                                                charCount2++;
                                            }
                                            if (charCount2 == 2)
                                            {
                                                break;
                                            }
                                            week2CharList.Add(c);
                                        }
                                        char[] week2Array = week2CharList.ToArray();
                                        Array.Reverse(week2Array);
                                        string w2String = new string(week2Array);
                                        lastWeek = Convert.ToDouble(w2String.Substring(0, w2String.Length - 5));
                                        break;
                                    case 4:
                                        char[] nameArray = lines[i + 1].Trim().ToCharArray();
                                        Array.Reverse(nameArray);
                                        int charCount3 = 0;
                                        List<char> nameCharList = new List<char>();
                                        foreach (char c in nameArray)
                                        {
                                            if (c == '>')
                                            {
                                                charCount3++;
                                            }
                                            if (charCount3 == 3)
                                            {
                                                break;
                                            }
                                            nameCharList.Add(c);
                                        }
                                        char[] nameArray2 = nameCharList.ToArray();
                                        Array.Reverse(nameArray2);
                                        string nameString = new string(nameArray2);
                                        name = nameString.Substring(0, nameString.Length - 9);
                                        i++;
                                        break;
                                    case 5:
                                        average = Convert.ToDouble(lines[i].Trim().Substring(4, lines[i].Trim().Length - 9));
                                        break;
                                    case 6:
                                        total = Convert.ToDouble(lines[i].Trim().Substring(4, lines[i].Trim().Length - 9));
                                        break;
                                    default:
                                        break;
                                }
                                tdCounter++;
                                i++;
                            }
                            GolfRank rank = new GolfRank(name, thisWeek, lastWeek, average, total);
                            golferRankings.Add(rank);
                        }
                        trCounter++;
                    }
                }
            }

            Console.WriteLine("Enter the field. Type 'f' when finished.");
            Console.WriteLine();

            string enterdName = "";
            while (enterdName != "f")
            {
                Console.Write("Enter Golfer: ");
                enterdName = Console.ReadLine().Trim();

                if (enterdName != "f")
                {
                    MastersGolfer golfer = new MastersGolfer(enterdName);
                    var rank = golferRankings.Where(r => r.Name.ToLower() == golfer.Name.ToLower()).FirstOrDefault();

                    if (rank == null)
                    {
                        Console.WriteLine("No Golfer Found");
                    }
                    else
                    {
                        golfer.CurrentRank = rank;
                        mastersGolfers.Add(golfer);
                    }
                }
            }

            var orderedGolfers = mastersGolfers.OrderByDescending(g => g.CurrentRank.Score).ToList(); ;

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Enter each golfer drafted. Type 'q' to quit.");
            Console.WriteLine();


            while (orderedGolfers.Count > 0)
            {
                Console.Write("Selected Golfer: ");
                string selectedName = Console.ReadLine().Trim(); ;

                if(selectedName == "q")
                {
                    break;
                }

                var index = orderedGolfers.FindIndex(g => g.Name.ToLower() == selectedName.ToLower());

                if(index > -1)
                {
                    orderedGolfers.RemoveAt(index);
                    Console.WriteLine();
                    Console.WriteLine("Next Best: " + orderedGolfers[0].CurrentRank.Name + " Rank: " + orderedGolfers[0].CurrentRank.ThisWeek + " Avg: " + orderedGolfers[0].CurrentRank.AveragePoints);
                    var notTop20 = orderedGolfers.Where(g => g.CurrentRank.ThisWeek > 20).FirstOrDefault();
                    if (notTop20 != null)
                    {
                        Console.WriteLine("Next Best Not Top 20: " + notTop20.CurrentRank.Name + " Rank: " + notTop20.CurrentRank.ThisWeek + " Avg: " + notTop20.CurrentRank.AveragePoints);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Golfer Not Found");
                    Console.WriteLine();
                }
            }
        }
    }
}
