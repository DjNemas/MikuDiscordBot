﻿using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;

namespace MikuDiscordBot.Database
{
    public class DiscordDBContext : DbContext
    {
        public DbSet<Models.Credentials> DiscordApiConfigs { get; set; }
        public DbSet<Models.GuildInfo> GuildInfo { get; set; }

        public DbSet<Models.Playlist> Playlists { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                $"Data Source={Files.dbDiscordAbsolutPath};");
        }
    }
}
