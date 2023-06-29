using System;
using System.IO;

namespace SCAuditStudio.Classes.ProjectFile
{
    public class ProjectFile
    {
        public static string Appdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string SCAuditProjectsFolderName = "SCASProjects";

        public DateTime LastTimeOpened;
        public string Name;
        public string path;
        public string JudgeCommentFile;

        public ProjectFile(string pName,string pPath,string pJudgeCommentFile)
        {
            Name = pName;
            path = pPath;
            LastTimeOpened = DateTime.Now;
            JudgeCommentFile = pJudgeCommentFile;
        }
    }
}
