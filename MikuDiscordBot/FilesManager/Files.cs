using MikuDiscordBot.FilesManager.Models;
using System.Text.Json;

namespace MikuDiscordBot.FilesManager
{
    public static class Files
    {
        // DB All
        private static readonly string dbDir = "database";
        // DB Discord
        private static readonly string dbDiscordFile = "discord.db";
        public static readonly string dbDiscordRelativePath = Path.Combine(dbDir, dbDiscordFile);
        public static readonly string dbDiscordAbsolutPath = Path.Combine(Environment.CurrentDirectory, dbDiscordRelativePath);
        // Log
        private static readonly string logDir = "logs";
        private static readonly string logFile = "log.txt";
        public static readonly string logRelativePath = Path.Combine(logDir, logFile);
        public static readonly string logAbsolutPath = Path.Combine(Environment.CurrentDirectory, logRelativePath);
        // ConfigFile
        private static readonly string configDir = "config";
        private static readonly string configFile = "config.json";
        public static readonly string configRelativePath = Path.Combine(configDir, configFile);
        public static readonly string configAbsolutPath = Path.Combine(Environment.CurrentDirectory, configRelativePath);
        // Music Folder
        private static readonly string musicDir = "music";
        public static readonly string musicRelativePath = Path.Combine(musicDir);
        public static readonly string musicAbsolutPath = Path.Combine(Environment.CurrentDirectory, musicRelativePath);

        public static void EnsureConfigFileExist()
        {
            if(!File.Exists(configAbsolutPath))
            {
                var config = new ConfigJson();
                string json = JsonSerializer.Serialize(config, options: new JsonSerializerOptions()
                {
                    WriteIndented = true,
                });
                File.WriteAllText(configAbsolutPath, json);
            }
        }

        public static ConfigJson? GetConfigFile()
        {
            return JsonSerializer.Deserialize<ConfigJson>(File.ReadAllText(configAbsolutPath));
        }

        public static void EnsureAllFolderExist()
        {
            // Database
            EnsureFolderExist(new FileInfo(dbDiscordAbsolutPath));
            // Log
            EnsureFolderExist(new FileInfo(logAbsolutPath));
            // Config
            EnsureFolderExist(new FileInfo(configAbsolutPath));
            // Music
            EnsureFolderExist(new DirectoryInfo(musicAbsolutPath));
        }

        public static void EnsureFolderExist(DirectoryInfo dir)
        {
            if (!Directory.Exists(dir.FullName))
                Directory.CreateDirectory(dir.FullName);
        }

        public static void EnsureFolderExist(FileInfo file)
        {
            if (!Directory.Exists(file.FullName) && file.DirectoryName is not null)
                Directory.CreateDirectory(file.DirectoryName);
        }

        public static void DeleteAllInDir(DirectoryInfo file)
        {
            foreach (var item in file.GetDirectories())
            {
                item.Delete(true);
            }
            foreach (var item in file.GetFiles())
            {
                item.Delete();
            }
        }
    }
}
