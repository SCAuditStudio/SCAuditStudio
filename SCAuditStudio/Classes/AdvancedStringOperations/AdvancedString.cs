using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
