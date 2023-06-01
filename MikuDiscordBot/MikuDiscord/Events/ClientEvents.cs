using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;
using MikuDiscordBot.MikuDiscord.Models;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.Events
{
    public class ClientEvents
    {
        private readonly DiscordDBContext db;
        private readonly InteractionService interaction;

        public ClientEvents(DiscordDBContext db, InteractionService interaction)
        {
            this.db = db;
            this.interaction = interaction;
        }

        public async Task GuildAvailable(SocketGuild guild)
        {
            try
            {
                var commands = await interaction.RegisterCommandsToGuildAsync(guild.Id, true);
                await Console.Out.WriteLineAsync("Commands Registred to Guild: " + guild.Name);
            }
            catch (Exception ex) 
            {
                await Console.Out.WriteLineAsync("Error on command registration.\n" + ex);
            }

            new MusicSystem(guild.Id);
            await new PlaylistManager(db, guild.Id).LoadGuildPlaylist();
        }

        public async Task ButtonExecuted(SocketMessageComponent arg)
        {
            switch (arg.Data.CustomId)
            {
                case "DeletePlaylistNo":
                    await DeletePlaylistNo(arg);
                    break;            
                default:
                {
                    // Special button Cases
                    if (arg.Data.CustomId.Contains("DeletePlaylistYes"))
                        await DeletePlaylistYes(arg);
                    break;
                }
            }
        }

        public async Task SelectMenuExecuted(SocketMessageComponent arg)
        {
            switch (arg.Data.CustomId)
            {
                case "SelectPlaylist":
                    await SelectPlaylist(arg);
                    break;
                case "AddSong":
                    await AddSong(arg);
                    break;
                case "DeletePlaylist":
                    await DeletePlaylist(arg);
                    break;
            }
        }

        private async Task DeletePlaylistYes(SocketMessageComponent arg)
        {
            var values = arg.Data.CustomId.Split(',', StringSplitOptions.TrimEntries);
            int playlistID = int.Parse(values[1]);

            var playlist = db.GuildInfo
                .Include(pls => pls.Playlists)
                .ThenInclude(s => s.Songs)
                .First(guild => guild.GuildID == arg.GuildId)
                .Playlists
                .First(pl => pl.ID == playlistID);

            if(playlist.PlaylistName == "Default")
            {
                await arg.UpdateAsync(o =>
                {
                    o.Content = $"Playlist `Default` can't be deleted.";
                    o.Components = null;
                });
                return;
            }

            string playlistName = playlist.PlaylistName;

            db.RemoveRange(playlist.Songs);
            db.Remove(playlist);
            try
            {
                await db.SaveChangesAsync();
                if(arg.GuildId is not null)
                    PlaylistManager.GuildPlaylist[(ulong)arg.GuildId].UpdateAllPlaylistForGuild();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("[Error] Could not delete Playlist.\n" + ex);
                await Errors.ReportErrorUpdate(102, arg);
                return;
            }
            await arg.UpdateAsync(o =>
            {
                o.Content = $"Playlist `{playlistName}` deleted.";
                o.Components = null;
            });
        }

        private async Task DeletePlaylistNo(SocketMessageComponent arg)
        {
            await arg.UpdateAsync(o =>
            {
                o.Content = "Alright! No playlist deleted.";
                o.Components = null;
            });
        }

        private async Task DeletePlaylist(SocketMessageComponent arg)
        {            
            string[] values = arg.Data.Values.ElementAt(0).Split(",", StringSplitOptions.TrimEntries);
            uint playlistID = uint.Parse(values[0]);

            var button1 = new ButtonBuilder();
            button1.WithLabel("Yes");
            button1.WithStyle(ButtonStyle.Success);
            button1.WithCustomId($"DeletePlaylistYes,{values[0]}");

            var button2 = new ButtonBuilder();
            button2.WithLabel("No");
            button2.WithStyle(ButtonStyle.Danger);
            button2.WithCustomId("DeletePlaylistNo");

            var builder = new ComponentBuilder();
            builder.WithButton(button1);
            builder.WithButton(button2);

            await arg.UpdateAsync(o =>
            {
                o.Content = "Are your sure u want to Delete?\n" +
                $"Playlist: {values[1]}";
                o.Components = builder.Build();
            });
        }

        private async Task AddSong(SocketMessageComponent arg)
        {
            string[] values = arg.Data.Values.ElementAt(0).Split(",", StringSplitOptions.TrimEntries);
            uint playlistID = uint.Parse(values[0]);

            ulong guildID = arg.GuildId ?? 0;
            if(guildID == 0) // This should never happend. Implemented just in case to Debug
            {
                await Errors.ReportErrorUpdate(100, arg);
                return;
            }

            var songInfo = await GetSongInfo(guildID);
            if(songInfo is null)
            {
                await Errors.ReportErrorUpdate(101, arg);
                return;
            }
            await AddSongToDB(guildID, playlistID, songInfo);

            await arg.UpdateAsync(o =>
            {
                o.Content = $"Song `{songInfo.Title}` added to playlist `{values[1]}`";
                            o.Components = null;
            });
        }

        private async Task AddSongToDB(ulong guildID, uint playlistID, SongJsonInfo songInfo)
        {
            var playlist = db.GuildInfo.Include(pl => pl.Playlists)
                .First(guild => guild.GuildID == guildID)
                .Playlists
                .First(pl => pl.ID == playlistID);
            playlist.Songs.Add(new Song()
            {
                Title = songInfo.Title,
                VideoID = songInfo.VideoID,
                VideoURL = songInfo.VideoUrl
            });
            await db.SaveChangesAsync();
        }

        private async Task<SongJsonInfo?> GetSongInfo(ulong guildID)
        {
            using (var stream = Files.GetGuildMetadataFile(guildID))
                return await JsonSerializer.DeserializeAsync<SongJsonInfo>(stream);
        }

        private async Task SelectPlaylist(SocketMessageComponent arg)
        {
            string[] values = arg.Data.Values.ElementAt(0).Split(",", StringSplitOptions.TrimEntries);
            uint playlistID = uint.Parse(values[0]);

            if (arg.GuildId is not null)
                await PlaylistManager.GuildPlaylist[(ulong)arg.GuildId].ChangePlaylist(playlistID);

            await arg.UpdateAsync(sm =>
            {
                sm.Content = $"Playlist Selected: {values[1]}";
                sm.Components = null;
            });
        }
    }
}
