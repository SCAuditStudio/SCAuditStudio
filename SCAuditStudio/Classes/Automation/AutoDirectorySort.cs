using System;
using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.IO;

namespace SCAuditStudio
{
    static class AutoDirectorySort
    {
        public static async Task<int[]> GetScore(MDFile[] issues, string criteria)
        {
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

                Console.Write(response);
            }
            return new int[3] { 1, 1, 1 };
        }

        static async Task<bool> CompareIssues(MDFile issue1, MDFile issue2, string context)
        {
            float staticDistance = CompareIssuesStatic(issue1,issue2);
            if (staticDistance < 0.5)
            {
                return true;
            }
            else
            {
                string[] userMessage = new string[4];
                userMessage[0] = "Compare these two Smart Contract Vurnabilitys, only return one of these categorys: Same (if they are identical),Similar (if they they have something in common), Different(if they have nothing in common) use following context:";
                userMessage[1] = "Context: \n" + context;
                userMessage[2] = "Vulnability1: \n" + issue1.summary;
                userMessage[3] = "Vulnability2: \n" + issue1.summary;

                var userMessageS = userMessage.ToSingle();
                string response = await AISort.AskGPT(userMessageS);

                if (response.Contains("Same") || response.Contains("Similar"))
                {
                    return true;
                }
                if (response.Contains("Different"))
                {      
                    return false;
                }
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
                Console.WriteLine(blackListScore);
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
