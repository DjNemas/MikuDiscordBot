using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;

namespace MikuDiscordBot.Database
{
    public class DiscordDBContext : DbContext
    {
        public DbSet<Models.Credentials> DiscordApiConfigs { get; set; }
        public DbSet<Models.GuildInfo> GuildInfo { get; set; }

        public DbSet<Models.Playlist> Playlists { get; set; }

        private string dbAbsolutPath = Files.dbDiscordAbsolutPath;

        public DiscordDBContext() { }

        public DiscordDBContext(string dbAbsolutPath)
        {
            this.dbAbsolutPath = dbAbsolutPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                $"Data Source={dbAbsolutPath};");

            var eventIDs = new (EventId, LogLevel)[] { (CoreEventId.ManyServiceProvidersCreatedWarning, LogLevel.Information) };

            optionsBuilder.ConfigureWarnings(w =>
            {
                w.Log(eventIDs);
            });
        }
    }
}
