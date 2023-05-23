using System;

namespace SCAuditStudio.Classes.ProjectFile
{
    public class ProjectFile
    {
        public DateTime LastTimeOpened;
        public string Name;
        public string path;
        public ProjectFile(string pName,string pPath)
        {
            Name = pName;
            path = pPath;
            LastTimeOpened = DateTime.Now;
        }
    }
}
