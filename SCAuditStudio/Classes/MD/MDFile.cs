using Avalonia.Media;
using System;
using System.IO;

#pragma warning disable IDE1006
namespace SCAuditStudio
{
    public class MDFile
    {
        public string path;
        public string subPath;
        public string rawContent;

        public string author;
        public string severity;
        public string title;
        public string summary;
        public string detail;
        public string impact;
        public CodeSnippet[] code;
        public string tools;
        public string recommendation;
        public uint score;
        public string fileName { get => Path.GetFileName(path); }
        public IBrush? highlight;

        /* CONSTRUCTORS */
        public MDFile()
        {
            path = "";
            subPath = "";
            rawContent = "";

            author = "";
            severity = "";
            title = "";
            summary = "";
            detail = "";
            impact = "";
            code = Array.Empty<CodeSnippet>();
            tools = "";
            recommendation = "";
            score = 0;
        }
        public MDFile(string path, string rawContent)
        {
            this.path = path;
            subPath = "";
            this.rawContent = rawContent;

            author = "";
            severity = "";
            title = "";
            summary = "";
            detail = "";
            impact = "";
            code = Array.Empty<CodeSnippet>();
            tools = "";
            recommendation = "";
            score = 0;
        }

        /* STATIC PROPERTIES */
        public static MDFile Invalid
        {
            get { return new MDFile("invalid", "invalid"); }
        }
        public static bool operator == (MDFile? a, MDFile? b)
        {
            if(a is null || b is null) return false;
            bool pathSimilar = a.path == b.path;
            bool contentSimilar = a.rawContent == b.rawContent;
            return pathSimilar && contentSimilar;
        }
        public static bool operator != (MDFile? a, MDFile? b)
        {
            if (a is null || b is null) return false;
            bool pathSimilar = a.path != b.path;
            bool contentSimilar = a.rawContent != b.rawContent;
            return pathSimilar || contentSimilar;
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(path, rawContent);
        }
    }
}
