using System;
using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

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
        static public List<MDFile[]>? GroupIssues(MDFile[]? issuesToCompare, MDFile[]? issuesToCompareWith)
        {
            if(issuesToCompare == null || issuesToCompareWith == null) return null;
            List<MDFile[]> groups = new List<MDFile[]>();
            List<MDFile> issuesnew = new List<MDFile>();
            issuesnew.AddRange(issuesToCompareWith);

            for (int i= 0; i< issuesToCompare.Length; i++)
            {
                List<MDFile> similar = new List<MDFile>();

                for (int j = i+1; j< issuesnew.Count; j++)
                {
                    if(CompareIssues(issuesToCompare[i], issuesnew[j]))
                    {
                        similar.Add(issuesnew[j]);
                        issuesnew.RemoveAt(j);
                    }
                }

                if (similar.Count > 0)
                {
                    similar.Add(issuesToCompare[i]);
                    groups.Add(similar.ToArray());
                }
            }
            return groups;
        }
        static bool CompareIssues(MDFile issue1, MDFile issue2)
        {
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
