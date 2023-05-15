using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCAuditStudio.Classes.ProjectFile
{
    static class ProjectFileReader
    {
        public static ProjectFile[] ReadProjects(string ProjectFilePath)
        {
            using (StreamReader r = new StreamReader("ProjectFilePath.json"))
            {
                string json = r.ReadToEnd();
                List<ProjectFile>? projects = JsonConvert.DeserializeObject<List<ProjectFile>>(json);
                if (projects == null) return Array.Empty<ProjectFile>();
                return projects.ToArray();
            }
        }

        public static void CreateProjectFile(List<ProjectFile> projectsData, string ProjectFilePath)
        {
            //open file stream
            using (StreamWriter file = File.CreateText(ProjectFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, projectsData);
            }
        }
    }
}
