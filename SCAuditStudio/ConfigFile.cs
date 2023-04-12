using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCAuditStudio
{
    public static class ConfigFile
    {
        static string file = @"C:\";
        static bool initialized;

        public static void Init()
        {
            file = Path.Combine("./", "config.txt");
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

        public static void Write(string name, object? value)
        {
            if (!initialized)
            {
                return;
            }

            CheckFile();

            List<string> content = File.ReadAllText(file).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
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
            File.AppendAllText(file, $"{arg}{Environment.NewLine}");
        }
        public static async Task WriteAsync(string name, object value)
        {
            if (!initialized)
            {
                return;
            }

            CheckFile();

            string fileContent = await File.ReadAllTextAsync(file);
            List<string> content = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
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
            await File.AppendAllTextAsync(file, $"{arg}{Environment.NewLine}");
        }

        public static T? Read<T>(string name)
        {
            if (!initialized)
            {
                return default;
            }

            CheckFile();

            List<string> content = File.ReadAllText(file).Split(Environment.NewLine).ToList();
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
            List<string> content = fileContent.Split(Environment.NewLine).ToList();
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
