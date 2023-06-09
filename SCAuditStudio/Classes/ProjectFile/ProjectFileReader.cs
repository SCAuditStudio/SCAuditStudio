﻿using Newtonsoft.Json;
using SCAuditStudio.Classes.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCAuditStudio.Classes.ProjectFile
{
    static class ProjectFileReader
    {
        public static string Appdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string SCAuditProjectsFolderName = "SCASProjects";
        public static string SCAuditJudgingCommentsFolderName = "SCASJudgingComments";
        public static string SCAuditProjectsFileName = "ProjectFilePath.json";

        public static ProjectFile[] ReadProjects()
        {
            string SCAuditProjectsPath = Path.Combine(Appdatafolder, SCAuditProjectsFolderName, SCAuditProjectsFileName);
            if (!Directory.Exists(Path.Combine(Appdatafolder, SCAuditProjectsFolderName)))
            {
               return Array.Empty<ProjectFile>();
            }
            if (!File.Exists(SCAuditProjectsPath))
            {
                File.Create(SCAuditProjectsPath).Close();
            }
            using StreamReader r = new(SCAuditProjectsPath);
            string json = r.ReadToEnd();
            List<ProjectFile>? projects = JsonConvert.DeserializeObject<List<ProjectFile>>(json);
            if (projects == null) return Array.Empty<ProjectFile>();
            return projects.ToArray();
        }
        public static void RemoveProjectFile(string directory)
        {
            List<ProjectFile> projects = ReadProjects().ToList();
            for (int i = 0; i < projects.Count; i++)
            {
                if (projects[i].path == directory)
                {
                    projects.RemoveAt(i);
                }
            }

            string SCAuditProjectsPath = Path.Combine(Appdatafolder, SCAuditProjectsFolderName, SCAuditProjectsFileName);
            if (!Directory.Exists(Path.Combine(Appdatafolder, SCAuditProjectsFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(Appdatafolder, SCAuditProjectsFolderName));
            }
            //open file stream
            using StreamWriter file = File.CreateText(SCAuditProjectsPath);
            JsonSerializer serializer = new ();
            //serialize object directly into file stream
            serializer.Serialize(file, projects.ToArray());
        }
        public static void CreateProjectFile(string directory)
        {
            ProjectFile[] projects = ReadProjects();
            ProjectFile currentOpenProject = new(Path.GetFileName(directory), directory, CSVManager.CSVFilePath);

            foreach (ProjectFile project in projects)
            {
                if (project.path == directory)
                {
                    return;
                }
            }

            projects = projects.Concat(new ProjectFile[1] { currentOpenProject }).ToArray();
            string SCAuditProjectsPath = Path.Combine(Appdatafolder, SCAuditProjectsFolderName, SCAuditProjectsFileName);

            if (!Directory.Exists(Path.Combine(Appdatafolder, SCAuditProjectsFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(Appdatafolder, SCAuditProjectsFolderName));
            }

            if (!Directory.Exists(CSVManager.CSVFolderPath))
            {
                Directory.CreateDirectory(CSVManager.CSVFolderPath);
            }

            if (!File.Exists(CSVManager.CSVFilePath))
            {
                File.Create(CSVManager.CSVFilePath).Close();
            }

            //open file stream
            using StreamWriter file = File.CreateText(SCAuditProjectsPath);
            JsonSerializer serializer = new();

            //serialize object directly into file stream
            serializer.Serialize(file, projects);

            //Add CVS File creation
            CSVManager.CreateCSVFile();
        }
    }
}
