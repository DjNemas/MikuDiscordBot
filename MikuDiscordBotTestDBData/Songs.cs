using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBotTestDBData
{
    internal class Songs
    {
        private DiscordDBContext db;

        public Songs()
        {
            this.db = new DiscordDBContext(@"G:\Visual Studio Projekte\C#\MikuDiscordBot\MikuDiscordBot\bin\Debug\net7.0\database\discord.db");
        }

        public void InsertDumpSongData(uint numberOfSongs, ulong guildID, uint playlistID)
        {
            var playlist = db.GuildInfo
                .Include(pls => pls.Playlists)
                .ThenInclude(s => s.Songs)
                .First(guild => guild.GuildID == guildID)
                .Playlists
                .First(pl => pl.ID == playlistID);

            for (int i = 0; i < numberOfSongs; i++)
            {
                playlist.Songs.Add(new Song()
                {
                    Title = $"【周小蚕】INTERNET YAMERO feat. 初音ミク",
                    VideoID = "S0GyAJpu2Lw",
                    VideoURL = @"https://www.youtube.com/watch?v=S0GyAJpu2Lw"
                });
            }
            db.SaveChanges();
        }

        public void OnlyGetRangeOfSong(int page, ulong guildID, uint playlistID)
        {
            db.Playlists.Include(s => s.Songs).Load(); // wtf -.-

            //db.Dispose();
            //db = new(@"G:\Visual Studio Projekte\C#\MikuDiscordBot\MikuDiscordBot\bin\Debug\net7.0\database\discord.db");

            var playlist = db.Playlists
                .Include(s => s.Songs)
                .First(pl => pl.ID == playlistID);

            playlist.Songs = playlist.Songs.Skip(25 * (page - 1)).Take(25).ToList();

            Console.WriteLine(playlist == null);
            Console.WriteLine(playlist?.Songs.Count);
        }
    }
}
