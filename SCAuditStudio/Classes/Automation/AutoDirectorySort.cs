using System;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace SCAuditStudio
{
    static class AutoDirectorySort
    {
        public static async Task<int> GetScore(MDFile issue, string criteria, string context, int avgIssueTextLength)
        {
            //Perform Static Checks
            int staticScore = GetStaticScore(issue, avgIssueTextLength);

            //Perform AI Checks
            string[] userMessage = new string[6];
            userMessage[0] = "Check if this Smart Contract Vurnability is an valid issue, only return: 'Invalid1' (if the issue is Invalid), 'Invalid2' (if there is a small chance the issue is Invalid), 'Valid' (if you are sure the issue is valid). Use following criteria and context:";
            userMessage[1] = "Criteria: \n" + criteria;
            userMessage[2] = "Context: \n" + context;
            userMessage[3] = "Vurnability Detail: \n" + issue.detail;
            userMessage[4] = "Vurnability Impact: \n" + issue.summary;
            userMessage[5] = "Vurnability Impact: \n" + issue.impact;

            var userMessageS = userMessage.ToSingle();
            string response = await AISort.AskGPT(userMessageS);

            if (response.Contains("Invalid1"))
            {
                staticScore *= 0;
            }
            if (response.Contains("Invalid2"))
            {
                staticScore *= 1;
            }
            if (response.Contains("Valid"))
            {
                staticScore *= 2;
            }
            return staticScore;
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

        static public int GetStaticScore(MDFile issue,float avgIssueTextLength)
        {
            int totallength = issue.impact.Length + issue.detail.Length + issue.summary.Length;
            int blackListScore = StaticStringOperations.CheckForBlackList(issue, ConfigFile.Read<string>("BlackList") ?? "");
            int totalscore = Convert.ToInt16((totallength / avgIssueTextLength) * (issue.impact.Length + blackListScore));
            return totalscore;
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
