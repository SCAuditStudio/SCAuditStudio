using SCAuditStudio.Classes.ProjectFile;
using SCAuditStudio.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCAuditStudio.Classes.Helpers
{
    static class CSVManager
    {
        public static string CSVFolderPath => Path.Combine(ProjectFileReader.Appdatafolder, ProjectFileReader.SCAuditProjectsFolderName, ProjectFileReader.SCAuditJudgingCommentsFolderName);
        public static string CSVFileName => Path.GetFileName(MainWindow.Instance?.GetViewModel()?.ProjectDirectory ?? string.Empty);
        public static string CSVFilePath => Path.Combine(CSVFolderPath, $"{CSVFileName}.csv");

        public static void WriteCommentToIssue(string comment, string issuePath)
        {
            List<string[]> data = ReadCSVFile();

            bool overwritten = false;
            foreach (string[] row in data)
            {
                if (row.Contains(issuePath))
                {
                    if (comment == string.Empty)
                    {
                        data.Remove(row);
                        overwritten = true;
                        break;
                    }

                    data[data.IndexOf(row)][1] = $"\"{comment}\"";
                    overwritten = true;
                    break;
                }
            }

            if (!overwritten) data.Add(new string[] { issuePath, comment });

            string text = string.Empty;
            for (int r = 0; r < data.Count; r++)
            {
                string[] columns = data[r];
                for (int c = 0; c < columns.Length; c++)
                {
                    columns[c] = $"\"{columns[c]}\"";
                }
                string row = string.Join(",", columns);
                text += row + (r < data.Count - 1 ? '\n' : string.Empty);
            }

            File.WriteAllText(CSVFilePath, text);
        }

        public static void ExportFormatedCSVFile(string resultFilePath)
        {
            //Read MD file -> save data in corret format
            //End CSV File should look like this: IssueName,Author,Severity,IssueTitle,JudgeComment
            //Path should be definded by user

            MDManager? mdManager = MainWindow.Instance?.GetViewModel()?.mdManager;
            if (mdManager == null) return;

            string csv = "\"Issue\",\"Folder\",\"Author\",\"Severity\",\"Title\",\"Comment\"\n";
            for (int f = 0; f < mdManager.mdFiles.Length; f++)
            {
                MDFile mdFile = mdManager.mdFiles[f];
                string[] row = new string[] { $"\"{mdFile.fileName}\"", $"\"{mdFile.subPath}\"", $"\"{mdFile.author}\"", $"\"{mdFile.severity}\"", $"\"{mdFile.title}\"", $"\"{mdFile.judgeComment}\"" };
                csv += string.Join(',', row) + (f < mdManager.mdFiles.Length - 1 ? '\n' : string.Empty);
            }

            File.WriteAllText(resultFilePath, csv);
        }

        public static void CreateCSVFile()
        {
            File.Create(CSVFilePath).Close();
        }

        public static string ReadCommentOfIssue(string issue)
        {
            List<string[]> data = ReadCSVFile();

            foreach (string[] rowData in data)
            {
                if (rowData.Length < 2) continue;

                if (rowData[0] == issue)
                {
                    return rowData[1];
                }
            }

            return string.Empty;
        }

        public static List<string[]> ReadCSVFile()
        {
            // Read data from the CSV file
            List<string[]> data = new();

            if (!File.Exists(CSVFilePath)) return data;
            string text = File.ReadAllText(CSVFilePath);
            string[] rows = text.Split('\n');
            foreach (string row in rows)
            {
                string[] columns = row.Split(',');
                for (int c = 0; c < columns.Length; c++)
                {
                    columns[c] = columns[c].TrimStart('\"').TrimEnd('\"');
                }

                data.Add(columns);
            }

            return data;
        }
    }
}
