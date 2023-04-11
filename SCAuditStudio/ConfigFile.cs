using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
namespace SCAuditStudio
{
    public static class ConfigFile
    {
        static string file = @"C:\";
        static bool initialized;

        public static void Init()
        {
            file = Path.Combine(Application.StartupPath, "config.txt");
            CheckFile();

            initialized = true;
        }

        static void CheckFile()
        {
            if (!File.Exists(file))
            {
                File.Create(file).Close();
            }
        }
        public static string ToSingle(this List<string> list)
        {
            string result = $"{list[0]}\n";
            for (int i = 1; i < list.Count; i++)
            {
                result += $"{list[i]}\n";
            }
            return result;
        }

        public static void Write(string name, object? value)
        {
            if (!initialized)
            {
                return;
            }

            CheckFile();

            List<string> content = File.ReadAllText(file).Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            string arg = $"{name} : {value}";
            for (int i = 0; i < content.Count; i++)
            {
                string line = content[i];
                
                if (line.StartsWith(name))
                {
                    if (value == null) content.RemoveAt(i);
                    else content[i] = arg;

                    File.WriteAllText(file, content.ToSingle());
                    return;
                }
            }
            File.AppendAllText(file, $"{arg}\n");
        }
        public static async Task WriteAsync(string name, object value)
        {
            if (!initialized)
            {
                return;
            }

            CheckFile();

            string fileContent = await File.ReadAllTextAsync(file);
            List<string> content = fileContent.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            string arg = $"{name} : {value}";
            for (int i = 0; i < content.Count; i++)
            {
                string line = content[i];

                if (line.StartsWith(name))
                {
                    content[i] = arg;
                    await File.WriteAllTextAsync(file, content.ToSingle());
                    return;
                }
            }
            await File.AppendAllTextAsync(file, $"{arg}\n");
        }

        public static T? Read<T>(string name)
        {
            if (!initialized)
            {
                return default;
            }

            CheckFile();

            List<string> content = File.ReadAllText(file).Split("\n").ToList();
            for (int i = 0; i < content.Count; i++)
            {
                string line = content[i];

                if (line.StartsWith(name))
                {
                    return (T)Convert.ChangeType(line.Split(" : ")[^1], typeof(T));
                }
            }

            return default;
        }
        public static async Task<T?> ReadAsync<T>(string name)
        {
            if (!initialized)
            {
                return default;
            }

            CheckFile();

            string fileContent = await File.ReadAllTextAsync(file);
            List<string> content = fileContent.Split("\n").ToList();
            for (int i = 0; i < content.Count; i++)
            {
                string line = content[i];

                if (line.StartsWith(name))
                {
                    return (T)Convert.ChangeType(line.Split(" : ")[^1], typeof(T));
                }
            }

            return default;
        }
    }
}
