using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SCAuditStudio
{
    public static class MDReader
    {
        static readonly int authorLine   = 0;
        static readonly int severityLine = 2;
        static readonly int titleLine    = 4;
        static readonly string summaryHeader        = "## Summary";
        static readonly string detailHeader         = "## Vulnerability Detail";
        static readonly string impactHeader         = "## Impact";
        static readonly string codeHeader           = "## Code Snippet";
        static readonly string toolHeader           = "## Tool used";
        static readonly string recommendationHeader = "## Recommendation";
        
        /* INTERNAL FILE ANALYSIS */
        static string[] ParseWebLinks(string text)
        {
            Regex linkParser = new(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = linkParser.Matches(text);
            string[] links = new string[matches.Count];
            for (int l = 0; l < links.Length; l++)
            {
                Match match = matches[l];
                links[l] = match.Value;
            }

            return links;
        }
        static string[] GetOddIndexedElements(this string[] array)
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
        static CodeSnippet[] ParseCodeSnippets(MDFile file)
        {
            List<CodeSnippet> snippets = new();

            string[] rawSnippets = file.rawContent.Split("```").GetOddIndexedElements();
            foreach (string rawSnippet in rawSnippets)
            {
                string language = rawSnippet.Split('\r')[0];
                int startIndex = $"{language}\r".Length;
                if (startIndex >= rawSnippet.Length) continue;
                string snippet = rawSnippet[startIndex..].Trim();
                snippets.Add(new CodeSnippet(snippet, language));
            }

            return snippets.ToArray();
        }
        static async Task<CodeSnippet[]> ParseCodeLinks(MDFile file)
        {
            List<CodeSnippet> snippets = new();

            string[] links = ParseWebLinks(file.rawContent);
            foreach (string link in links)
            {
                if (link.Contains("github.com"))
                {
                    snippets.Add(await ParseGithubCodeAsync(link));
                }
            }

            return snippets.ToArray();
        }
        static async Task<CodeSnippet> ParseGithubCodeAsync(string githubLink)
        {
            string[] rawLink = githubLink.Replace("github.com", "raw.githubusercontent.com").Replace("blob/", "").Split("#");
            string link = rawLink[0];
            string[] lineInfo = rawLink[^1].Split("-");

            HttpClient client = new();
            string language = "undefined";
            string code = "";

            try
            {
                string response = await client.GetStringAsync(link);
                string[] page = response.Split("\n");

                int lineStart = rawLink.Length > 1 ? int.Parse(lineInfo[0].Replace("L", "")) - 1 : 0;
                int lineEnd = lineInfo.Length > 1 ? int.Parse(lineInfo[^1].Replace("L", "")) : page.Length;

                language = page.Where(l => l.StartsWith("pragma"))?.First().Split(" ")[1] ?? "undefined";
                code = "";

                for (int l = lineStart; l < lineEnd; l++)
                {
                    code += page[l];
                }
            }
            catch
            {
                return CodeSnippet.Empty;
            }

            return new CodeSnippet(code, language);
        }
        public static async Task<MDFile> ReadFileAsync(string file, bool ignoreLinks = false)
        {
            MDFile mdFile = new();

            //search file, return invalid if not found
            if (!File.Exists(file)) { return MDFile.Invalid; }

            mdFile.path = file;
            mdFile.rawContent = await File.ReadAllTextAsync(file);
            string[] lines = mdFile.rawContent.Split("\n");

            mdFile.author = lines[authorLine];
            mdFile.severity = lines[severityLine];
            mdFile.title = lines[titleLine][2..];

            int summaryIndex = mdFile.rawContent.IndexOf(summaryHeader);
            int detailIndex = mdFile.rawContent.IndexOf(detailHeader);
            int impactIndex = mdFile.rawContent.IndexOf(impactHeader);
            int codeIndex = mdFile.rawContent.IndexOf(codeHeader);
            int toolIndex = mdFile.rawContent.IndexOf(toolHeader);
            int recommendationIndex = mdFile.rawContent.IndexOf(recommendationHeader);

            mdFile.summary = mdFile.rawContent[(summaryIndex + summaryHeader.Length)..detailIndex].Trim();
            mdFile.detail = mdFile.rawContent[(detailIndex + detailHeader.Length)..impactIndex].Trim();
            mdFile.impact = mdFile.rawContent[(impactIndex + impactHeader.Length)..codeIndex].Trim();
            mdFile.tools = mdFile.rawContent[(toolIndex + toolHeader.Length)..recommendationIndex].Trim();
            mdFile.recommendation = mdFile.rawContent[(recommendationIndex + recommendationHeader.Length)..].Trim();

            CodeSnippet[] codeSnippets = ParseCodeSnippets(mdFile);
            CodeSnippet[] linkSnippets = ignoreLinks ? Array.Empty<CodeSnippet>() : await ParseCodeLinks(mdFile);
            mdFile.code = codeSnippets.Concat(linkSnippets).ToArray();

            return mdFile;
        }
    }
}
