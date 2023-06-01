using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.MusicEngine
{
    public class PlaylistManager
    {
        public static readonly Dictionary<ulong, PlaylistManager> GuildPlaylist = new();
        private ulong guildID;
        private readonly DiscordDBContext db;
        private readonly string defaultPlaylistName = "Default";
        private List<Playlist> playlistList = new List<Playlist>();
        public Playlist? selectedPlaylist { get; private set; } = null;

        public PlaylistManager(DiscordDBContext db, ulong guildID)
        {
            this.db = db;
            this.guildID = guildID;
        }

        public void UpdateAllPlaylistForGuild()
        {
            playlistList = db.GuildInfo
                .Include(pls => pls.Playlists)
                .ThenInclude(s => s.Songs)
                .First(guild => guild.GuildID == guildID)
                .Playlists;

            if (selectedPlaylist is not null && !playlistList.Contains(selectedPlaylist))
                selectedPlaylist = playlistList.First(pl => pl.PlaylistName == "Default");
        }

        public async void ChangePlaylist(Playlist playlist)
        {
            await ChangePlaylist(playlist.ID);
        }

        public async Task ChangePlaylist(uint id)
        {
            selectedPlaylist = await db.Playlists.FirstAsync(pl => pl.ID == id);
        }

        public async Task LoadGuildPlaylist()
        {
            await EnsureGuildInfoExist(guildID);
            await EnsureDefaultPlaylistExist(guildID);

            if(!GuildPlaylist.ContainsKey(guildID))
            {
                GuildPlaylist.Add(guildID, this);
            }

            var playlists = db.GuildInfo
                .Include(pl => pl.Playlists)
                .ThenInclude(s => s.Songs)
                .First(guild => guild.GuildID == guildID)
                .Playlists;

            if(playlists is not null)
            {
                playlistList = playlists;
                selectedPlaylist = playlistList.FirstOrDefault(pl => pl.PlaylistName == defaultPlaylistName);
            }
                
        }

        private async Task EnsureDefaultPlaylistExist(ulong guildID)
        {
            var playlist = db.GuildInfo
                .Include(pl => pl.Playlists)
                .First(guild => guild.GuildID == guildID).Playlists
                .FirstOrDefault(pl => pl.PlaylistName == defaultPlaylistName);
            
            if(playlist is null)
            {
                var newPlaylist = new Playlist()
                {
                    PlaylistName = defaultPlaylistName,
                };
                var guild = await db.GuildInfo.FirstOrDefaultAsync(guild => guild.GuildID == guildID);
                if(guild is not null)
                {
                    guild.Playlists.Add(newPlaylist);
                    await db.SaveChangesAsync();
                }
            }
        }
        private async Task EnsureGuildInfoExist(ulong guildID)
        {
            var guild = db.GuildInfo.FirstOrDefault(g => g.GuildID == guildID);
            if(guild is null)
            {
                var newGuild = new GuildInfo()
                {
                    GuildID = guildID,
                };
                await db.GuildInfo.AddAsync(newGuild);
                await db.SaveChangesAsync();
            }
        }
    }
}
