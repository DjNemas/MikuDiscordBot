using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;
using MikuDiscordBot.MikuDiscord;
using MikuDiscordBot.MikuDiscord.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikuDiscordBot.Interactions.ComponentInteraction
{
    public class MusicSong : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        private readonly DiscordDBContext db;

        public MusicSong(DiscordDBContext db)
        {
            this.db = db;
        }

        [ComponentInteraction("SongAdd")]
        public async Task SongAdd(string[] values)
        {
            string[] args = values[0].Split(",", StringSplitOptions.TrimEntries);
            uint playlistID = uint.Parse(args[0]);
            string playlistName = args[1];


            if (playlistID == 0) // This should never happend. Implemented just in case to Debug
            {
                await Errors.ReportErrorUpdate(100, Context.Interaction);
                return;
            }

            var songInfo = await GetSongInfo(Context.Guild.Id);
            if (songInfo is null)
            {
                await Errors.ReportErrorUpdate(101, Context.Interaction);
                return;
            }
            await AddSongToDB(Context.Guild.Id, playlistID, songInfo);

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = $"Song `{songInfo.Title}` added to playlist `{playlistName}`";
                o.Components = null;
            });
        }

        #region HelperFunctions
        private async Task<SongJsonInfo?> GetSongInfo(ulong guildID)
        {
            using (var stream = Files.GetGuildMetadataFile(guildID))
                return await JsonSerializer.DeserializeAsync<SongJsonInfo>(stream);
        }

        private async Task AddSongToDB(ulong guildID, uint playlistID, SongJsonInfo songInfo)
        {
            var playlist = db.GuildInfo
                .Include(pl => pl.Playlists)
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
        #endregion
    }
}
