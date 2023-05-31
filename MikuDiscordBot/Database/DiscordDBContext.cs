using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.Database
{
    public class DiscordDBContext : DbContext
    {
        public DbSet<Models.Credentials> DiscordApiConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                $"Data Source={Files.dbDiscordAbsolutPath};");
        }
    }
}
