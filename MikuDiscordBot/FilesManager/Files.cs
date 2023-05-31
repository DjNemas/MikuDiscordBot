using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MikuDiscordBot.FilesManager.Models;

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
        }

        private static void EnsureFolderExist(FileInfo file)
        {
            if (!Directory.Exists(file.FullName) && file.DirectoryName != null)
                Directory.CreateDirectory(file.DirectoryName);
        }
    }
}
