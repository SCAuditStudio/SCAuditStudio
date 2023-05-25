using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SCAuditStudio
{
    static class AutoDirectorySort
    {
        static string regexCodeLink = @"(.+)\.sol(?:#)?L+(\d+)(?:.+)?(?:-L+(\d+)(?:.+)?)";

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
            int staticLinkDistance = 0;

            if (staticDistanceTitle <= 0.56)
            {
                return true;
            }

            for (int i = 0; i < issue1.links.Length; i++)
            {
                for (int j = 0; j < issue2.links.Length; j++)
                {
                    if (CompareLinks(issue1.links[i], issue2.links[j]))
                    {
                        staticLinkDistance++;
                    }
                }
            }
            if (staticLinkDistance == 0)
            {
                return false;
            }

            float per = (issue1.links.Length + issue2.links.Length) / staticLinkDistance;
            if (per > 0.8)
            {
                return true;
            }

            return false;
        }
        static bool CompareLinks(string link1, string link2)
        {
            string trimedlink1 = link1.Split('/')[^1];
            string trimedlink2 = link2.Split('/')[^1];

            Match match1 = Regex.Match(trimedlink1, regexCodeLink, RegexOptions.Singleline | RegexOptions.Compiled);
            string filename1 = match1.Groups[1].Value;

            int? startline1 = match1.Groups.Count > 2 ? int.Parse(match1.Groups[2].Value) : null;
            int? endline1 = match1.Groups.Count > 3 ? int.Parse(match1.Groups[3].Value) : startline1;

            Match match2 = Regex.Match(trimedlink2, regexCodeLink, RegexOptions.Singleline | RegexOptions.Compiled);
            string filename2 = match2.Groups[1].Value;
            int? startline2 = match2.Groups.Count > 2 ? int.Parse(match2.Groups[2].Value) : null;
            int? endline2 = match2.Groups.Count > 3 ? int.Parse(match2.Groups[3].Value) : startline2;

            if (filename1 != filename2)
            {
                return false;
            }

            if (startline1 == null || startline2 == null)
            {
                return false;
            }
            if (endline1 == startline1)
            {
                if (startline1 >= startline2 && startline1 <= endline2)
                {
                    return true;
                }
            }
            if (endline2 == startline2)
            {
                if (startline2 >= startline1 && startline2 <= endline1)
                {
                    return true;
                }
            }
            if (startline1 == startline2 && endline1 == endline2)
            {
                return true;
            }
            if (Math.Abs((float)(startline1 - startline2)) <= 10 && Math.Abs((float)(endline1 ?? startline1 - endline2 ?? startline2)) <= 10)
            {
                return true;
            }
            return false;
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

    }
}
