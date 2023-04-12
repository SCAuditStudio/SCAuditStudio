using System;
using System.Collections.Generic;

namespace SCAuditStudio
{
    public static class AdvancedString
    {
        public static string ToSingle(this string[] array)
        {
            string result = array[0];

            for (int l = 1; l < array.Length; l++)
            {
                result += array[l];
            }

            return result;
        }

        public static string ToSingle(this List<string> list)
        {
            string result = $"{list[0]}{Environment.NewLine}";
            for (int i = 1; i < list.Count; i++)
            {
                result += $"{list[i]}{Environment.NewLine}";
            }
            return result;
        }

        public static string[] GetOddIndexedElements(this string[] array)
        {
            List<string> result = new();

            for (int l = 0; l < array.Length; l++)
            {
                if (l % 2 != 0)
                {
                    result.Add(array[l]);
                }
            }

            return result.ToArray();
        }

        public static int[] IndexesOf(this string s, char c)
        {
            List<int> indexes = new();

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c) indexes.Add(s[i]);
            }

            return indexes.ToArray();
        }

        public static int[] IndexesOf(this string a, string str)
        {
            List<int> indexes = new();

            for (int i = 0; i < a.Length; i++)
            {
                if (i + str.Length > a.Length) break;

                for (int j = 0; j < str.Length; j++)
                {
                    if (a[i+j] != str[j]) break;
                    if (j == str.Length - 1) indexes.Add(j);
                }
            }

            return indexes.ToArray();
        }
    }
}
