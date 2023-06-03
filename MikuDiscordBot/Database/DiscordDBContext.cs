using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

            optionsBuilder.UseMemoryCache(new MemoryCache(
                new MemoryCacheOptions()
                {
                    SizeLimit = 0,
                }));
        }

        public static DbContextOptionsBuilder? GetDBOptions()
        {
            var dbOptionsBuilder = new DbContextOptionsBuilder();
            return dbOptionsBuilder.UseMemoryCache(new MemoryCache(
                new MemoryCacheOptions()
                {
                    SizeLimit = 0,
                }));
        }
    }
}
