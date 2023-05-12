using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using AvaloniaEdit.Utils;
using System.Linq;

namespace SCAuditStudio
{
    static class AutoDirectorySort
    {
        public static async Task<int[]> GetScore(MDFile[]? issues, string criteria)
        {
            if (issues == null) return Array.Empty<int>();

            //Perform Static Checks


            for (int i = 0; i < issues.Length; i +=2) {
                //Perform AI Checks
                string[] userMessage = new string[8];
                userMessage[0] = "Check if these Smart Contract Vurnabilitys are valid issues, only return: 'Invalid' (if the issue is Invalid),'Valid' (if you are sure the issue is valid) for each issue. Use following criteria:";
                userMessage[1] = "Criteria: \n" + criteria;
                //userMessage[2] = "Context: \n" + context;
                userMessage[2] = "Vurnability1 Summary: \n" + issues[i].summary;
                userMessage[3] = "Vurnability1 Impact: \n" + issues[i].impact;
                if (i+1 <= issues.Length)
                {
                    userMessage[4] = "Vurnability2 Summary: \n" + issues[i + 1].summary;
                    userMessage[5] = "Vurnability2 Impact: \n" + issues[i + 1].impact;
                }
                var userMessageS = userMessage.ToSingle();
                string response = await AISort.AskGPT(userMessageS);

            }
            return new int[3] { 1, 1, 1 };
        }

        public static List<List<MDFile>>? GroupIssuesThreaded(MDFile[]? issuesToCompare, MDFile[]? issuesToCompareWith, int maxNumOfThreads)
        {
            if(issuesToCompareWith == null || issuesToCompare == null) return null;
            DateTime start = DateTime.Now;
            List<Thread> threads = new ();
            int step = (int)Math.Round(issuesToCompare.Length/ (float)maxNumOfThreads);
            List<List<List<MDFile>>?>? mdFiles = new();
            List<List<MDFile>>? result = new();

            for (int i = 0; i < issuesToCompare.Length; i+=step)
            {
                if ((issuesToCompare.Length - i) < step)
                {
                    MDFile[] files = issuesToCompare[i..(issuesToCompare.Length-1)];
                    Thread t = new(() => { mdFiles.Add(GroupIssues(files, issuesToCompareWith)); });
                    threads.Add(t);
                    t.Start();
                }
                else
                {
                    MDFile[] files = issuesToCompare[i..(i + step)];
                    Thread t = new(() => { mdFiles.Add(GroupIssues(files, issuesToCompareWith)); });
                    threads.Add(t);
                    t.Start();
                }
            }
            
            for (int t = 0; t < threads.Count; t++)
            {
                while (threads[t].IsAlive)
                {

                }
            }

            for(int t = 0; t < mdFiles.Count; t++)
            {
                if (mdFiles[t] == null)
                {
                    continue;
                }

                for (int i = 0; i < mdFiles[t]?.Count; i++)
                {
                    if (mdFiles[t]?[i] != null)
                    {
                        result.Add(mdFiles[t]![i]);
                    }
                }
            }

            Console.WriteLine("Time: " + (DateTime.Now - start).TotalSeconds);
            return result;
        }

        static public List<List<MDFile>>? GroupIssues(MDFile[]? issuesToCompare, MDFile[]? issuesToCompareWith)
        {
            if(issuesToCompare == null || issuesToCompareWith == null) return null;
            List<List<MDFile>> groups = new();

            for (int i= 0; i< issuesToCompare.Length; i++)
            {
                List<MDFile> similar = new();
                bool grouped = false;

                for (int g = 0; g < groups.Count; g++)
                {
                    if (CompareIssues(issuesToCompare[i], groups[g][0]))
                    {
                        groups[g].Add(issuesToCompare[i]);
                        grouped = true;
                        break;
                    }
                }

                if (grouped) continue;

                for (int j = i+1; j < issuesToCompareWith.Length; j++)
                {
                    if(CompareIssues(issuesToCompare[i], issuesToCompareWith[j]))
                    {
                        similar.Add(issuesToCompareWith[j]);
                    }
                }

                if (similar.Count > 1)
                {
                    similar.Add(issuesToCompare[i]);
                    groups.Add(similar);
                }
            }
            
            return groups;
        }
        static bool CompareIssues(MDFile issue1, MDFile issue2)
        {
            if(issue1 == MDFile.Invalid || issue2 == MDFile.Invalid) return false;
            if (Object.Equals(issue1, null) || Object.Equals(issue2, null)) return false;
            if (issue1.rawContent.Length < 1 || issue2.rawContent.Length < 1) return false;
            if (issue1.title.Length < 1 || issue2.title.Length < 1) return false;
            
            float staticDistanceTitle = StaticStringOperations.StaticCompareString(issue1.title, issue2.title);

            //Static compare content
            float staticDistanceContent = StaticStringOperations.StaticCompareString(issue1.rawContent, issue2.rawContent);
            
            if (staticDistanceTitle < 0.56 || staticDistanceContent < 0.45)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public void SetStaticScore(MDFile[] issues)
        {
            //int totallength = issue.impact.Length + issue.detail.Length + issue.summary.Length;
            float totallength = 0;
            string? blacklistPath = ConfigFile.Read<string>("BlackList");
            string blacklist = File.Exists(blacklistPath) ? File.ReadAllText(blacklistPath) : "";
            for (int i = 0; i < issues.Length; i++)
            {
                totallength += issues[i].impact.Length + issues[i].detail.Length + issues[i].summary.Length;
            }
            float avgissuelength = totallength / issues.Length;
            for (int i = 0; i < issues.Length; i++)
            {
                MDFile issue = issues[i];
                int totallengthIssue = issue.impact.Length + issue.detail.Length + issue.summary.Length;
                int blackListScore = StaticStringOperations.CheckForBlackList(issue, blacklist);
                uint totalscore = (UInt32)((totallengthIssue / avgissuelength)*100 / (blackListScore+1));
                issues[i].score = totalscore;
            }
        }
        static public float CompareIssuesStatic(MDFile issue1, MDFile issue2)
        {


            //Static compare title
            float staticDistanceTitle = StaticStringOperations.StaticCompareString(issue1.title, issue2.title);

            //Static compare content
            float staticDistanceContent = StaticStringOperations.StaticCompareString(issue1.rawContent, issue2.rawContent);
            return staticDistanceTitle + staticDistanceContent;
        }
    }
}
