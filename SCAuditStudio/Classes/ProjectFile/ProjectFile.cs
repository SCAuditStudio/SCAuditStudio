using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
