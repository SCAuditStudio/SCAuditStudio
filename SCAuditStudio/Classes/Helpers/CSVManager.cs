using System;
using System.Collections.Generic;
using System.IO;

namespace SCAuditStudio.Classes.Helpers
{
    static class CSVManager
    {

        public static void WriteCommentToIssue(string comment,string issuePath, string csvFilePath)
        {
            List<string[]> data = ReadCSVFile(csvFilePath);

            data.Add(new string[] { comment, issuePath });

            // Write data to the CSV file
            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                foreach (string[] rowData in data)
                {
                    string row = string.Join(",", rowData);
                    writer.WriteLine(row);
                }
            }
        }

        public static void ExportFormatedCSVFile(string csvFilePath, string resultFilePath)
        {
            List<string[]> data = ReadCSVFile(csvFilePath);

            foreach (string[] rowData in data)
            {
                //Read MD file -> save data in corret format
                //End CSV File should look like this: IssueName,Author,Severity,IssueTitle,JudgeComment
                //Path should be definded by user
            }

        }

        public static void CreateCSVFile(string csvFilePath)
        {
            File.Create(csvFilePath).Close();
        }

        public static string ReadCommentOfIssue(string path, string csvFilePath)
        {
            List<string[]> data = ReadCSVFile(csvFilePath);

            foreach (string[] rowData in data)
            {
                if (rowData[1] == path)
                {
                    return rowData[1];
                }
            }

            return "";
        }

        public static List<string[]> ReadCSVFile(string csvFilePath)
        {
  
            // Read data from the CSV file
            List<string[]> data = new List<string[]>();

            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] rowData = line.Split(',');
                    data.Add(rowData);
                }
            }

            return data;
        }
    }
}
