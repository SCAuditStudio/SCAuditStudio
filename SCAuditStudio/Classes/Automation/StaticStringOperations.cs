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
                if (issue.title.ToLower().Contains(blackListWords[i].ToLower()))
                {
                    totalWordsFound++;
                }
                if (issue.detail.ToLower().Contains(blackListWords[i].ToLower()))
                {
                    totalWordsFound++;
                }
                if (issue.impact.ToLower().Contains(blackListWords[i].ToLower()))
                {
                    totalWordsFound++;
                }
            }
            
            return totalWordsFound;
        }
        //Returns float between 0 and 1, 0 when same, 1 when different
        public static float StaticCompareString(string a, string b)
        {
            if (a.Length < 1 || b.Length < 1)
            {
                return 0;
            }
            float damerauLevenshteinDistance = GetDamerauLevenshteinDistance(a, b);
            float staticDistance = damerauLevenshteinDistance / Math.Max(a.Length, b.Length);
            return staticDistance;
        }
        //Returns 0 if the same, returns lenght of longer string if everything is different 
        public static int GetDamerauLevenshteinDistance(string s, string t)
        {
            if (s == null)
            {
                throw new ArgumentNullException(s, "String Cannot Be Null");
            }

            if (t == null)
            {
                throw new ArgumentNullException(t, "String Cannot Be Null");
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
                (d, p) = (p, d); //placeholder to assist in swapping p and d
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return p[n];
        }
    }
}
