using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCAuditStudio
{
    public class MDManager
    {
        readonly string directory = @"C:\";
        readonly string invalidFolder = "invalid";

        public enum MDFileIssue { Medium, High }

        public MDFile[] mdFiles = Array.Empty<MDFile>();
        
        public MDManager(string directory)
        {
            this.directory = directory;
        }

        /* INTERNAL FUNCTIONS */
        void IMoveFileTo(string name, string subPath)
        {
            //Only move .md files
            if (!name.EndsWith(".md")) return;

            //Get file
            MDFile? mdFile = GetFile(name);
            if (mdFile == null) return;
            if (mdFile.subPath == subPath) return;

            //Create subdirectory if necessary
            string dir = Path.Combine(directory, subPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //Move file to location
            string oldPath = Path.GetDirectoryName(mdFile.path) ?? "";
            string path = Path.Combine(dir, name);
            File.Move(mdFile.path, path);
            mdFile.subPath = subPath;
            mdFile.path = path;

            //Try remove folder if empty
            if (Directory.GetFiles(oldPath).Length == 0)
            {
                Directory.Delete(oldPath);
            }

            //Unmark file
            UnmarkFile(mdFile.fileName);
        }
        string IRenameFileTo(string name, string newName)
        {
            //Only rename .md files
            if (!name.EndsWith(".md")) return name;

            //Get file
            MDFile? mdFile = GetFile(name);
            if (mdFile == null) return name;

            string? dir = Path.GetDirectoryName(mdFile.path);
            if (dir == null) return name;

            //Rename file
            string path = Path.Combine(dir, newName);
            File.Move(mdFile.path, path);
            mdFile.path = path;

            return newName;
        }

        /* PUBLIC FUNCTIONS */
        public MDFile? GetFile(string name)
        {
            foreach (MDFile mdFile in mdFiles)
            {
                if (Path.GetFileName(mdFile.path) == name)
                {
                    return mdFile;
                }
            }

            return null;
        }
        public string[] GetSubDirectories()
        {
            if (!Directory.Exists(directory)) return Array.Empty<string>();
            return Directory.GetDirectories(directory);
        }
        public MDFile[] GetFilesInSubPath(string subPath)
        {
            return mdFiles.Where(f => f.subPath == subPath).ToArray();
        }
        public int GetFileCountInSubPath(string subPath)
        {
            return GetFilesInSubPath(subPath).Length;
        }
        public string[] GetIssues(MDFileIssue severity)
        {
            char severityLetter = severity == MDFileIssue.Medium ? 'M' : 'H';
            string[] issues = Directory.GetDirectories(directory, $"*-{severityLetter}");
            for (int i = 0; i < issues.Length; i++)
            {
                issues[i] = Path.GetFileName(issues[i]);
            }

            return issues;
        }
        public bool IssueExists(MDFileIssue severity, int index)
        {
            char severityLetter = severity == MDFileIssue.Medium ? 'M' : 'H';
            string issue = Path.Combine(directory, $"{index:000}-{severityLetter}");
            
            return Directory.Exists(issue);
        }
        public bool IssueExists(string issue)
        {
            if (!IsIssue(issue)) return false;
            return Directory.Exists(Path.Combine(directory, issue));
        }
        public int GetIssueIndex(MDFileIssue severity)
        {
            for (int i = 1; i < 999; i++)
            {
                if (!IssueExists(severity, i)) return i;
            }

            return 999;
        }
        public void MoveFileToRoot(string name)
        {
            IMoveFileTo(name, "");
        }
        public void MoveFileToInvalid(string name)
        {
            IMoveFileTo(name, invalidFolder);
        }
        public void MoveFileToIssue(string name, string issue, bool createNew)
        {
            //Check if Issue exists, if not - return
            if (!createNew && !IssueExists(issue)) return;

            IMoveFileTo(name, issue);
        }
        public void MoveFileToIssue(string name, MDFileIssue severity, int index, bool createNew)
        {
            //Check if Issue exists, if not - return
            if (!createNew && !IssueExists(severity, index)) return;

            char severityLetter = severity == MDFileIssue.Medium ? 'M' : 'H';
            string issue = $"{index:000}-{severityLetter}";

            IMoveFileTo(name, issue);
        }
        public string MarkFileAsBest(string name)
        {
            return IRenameFileTo(name, name.Replace(".md", "-best.md"));
        }
        public string UnmarkFile(string name)
        {
            return IRenameFileTo(name, name.Replace("-best.md", ".md"));
        }

        /* STATIC FUNCTIONS */
        public static bool IsIssue(string name)
        {
            return (name.EndsWith("-M") || name.EndsWith("-H")) && char.IsDigit(name[0]) && char.IsDigit(name[1]) && char.IsDigit(name[2]);
        }

        public async Task LoadFilesAsync()
        {
            List<MDFile> mdFileList = new();

            if (!Directory.Exists(directory))
            {
                return;
            }

            //Load files in root directory
            string[] files = Directory.GetFiles(directory, "*.md");
            foreach (string file in files)
            {
                MDFile mdFile = await MDReader.ReadFileAsync(file, true);
                mdFileList.Add(mdFile);
            }

            //Load files in subdirectories
            string[] subDirs = Directory.GetDirectories(directory);
            foreach (string subDir in subDirs)
            {
                files = Directory.GetFiles(subDir, "*.md");

                foreach (string file in files)
                {
                    MDFile mdFile = await MDReader.ReadFileAsync(file, true);
                    mdFile.subPath = Path.GetFileName(subDir);
                    mdFileList.Add(mdFile);
                }
            }

            mdFiles = mdFileList.OrderBy(f => Path.GetFileName(f.path)).ToArray();
            AutoDirectorySort.SetStaticScore(mdFiles);
        }
    }
}
