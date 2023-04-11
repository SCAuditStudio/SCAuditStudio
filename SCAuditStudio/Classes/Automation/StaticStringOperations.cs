using System;

namespace SCAuditStudio
{
    static class StaticStringOperations
    {
        //Returns amount of blacklisted words used in Issue
        public static int CheckForBlackList(MDFile issue, string blackList)
        {
            int totalWordsFound = 0;
            string[] blackListWords = blackList.Split(',');

            for (int i = 0; i < blackListWords.Length; i++)
            {
                if (issue.title.Contains(blackListWords[i]))
                {
                    totalWordsFound++;
                }
                if (issue.detail.Contains(blackListWords[i]))
                {
                    totalWordsFound++;
                }
                if (issue.impact.Contains(blackListWords[i]))
                {
                    totalWordsFound++;
                }
            }
            if (totalWordsFound == 0)
            {
                return 0;
            }
            else if (blackListWords.Length > totalWordsFound)
            {
                return totalWordsFound;
            }
            return 0;
        }
        //Returns float between 0 and 1, 0 when same, 1 when different
        public static float StaticCompareString(string title1, string title2)
        {
            float damerauLevenshteinDistance = GetDamerauLevenshteinDistance(title1, title2);
            float staticDistance = title1.Length > title2.Length ? damerauLevenshteinDistance / title1.Length : damerauLevenshteinDistance / title2.Length;
            return staticDistance;
        }
        //Returns 0 if the same, returns lenght of longer string if everything is different 
        public static int GetDamerauLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(s, "String Cannot Be Null Or Empty");
            }

            if (string.IsNullOrEmpty(t))
            {
                throw new ArgumentNullException(t, "String Cannot Be Null Or Empty");
            }

            int n = s.Length; // length of s
            int m = t.Length; // length of t

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            int[] p = new int[n + 1]; //'previous' cost array, horizontally
            int[] d = new int[n + 1]; // cost array, horizontally

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                char tJ = t[j - 1]; // jth character of t
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    int cost = s[i - 1] == tJ ? 0 : 1; // cost
                                                       // minimum of cell to the left+1, to the top+1, diagonally left and up +cost                
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                int[] dPlaceholder = p; //placeholder to assist in swapping p and d
                p = d;
                d = dPlaceholder;
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return p[n];
        }
    }
}
