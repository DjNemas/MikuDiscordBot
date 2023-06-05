using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;
using MikuDiscordBot.MikuDiscord;
using MikuDiscordBot.MikuDiscord.Models;
using MikuDiscordBot.MikuDiscord.MusicEngine;
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
        private readonly YTDLP ytDLP;
        private readonly MenuBuilder menuBuilder;

        public MusicSong(DiscordDBContext db, YTDLP ytDLP, MenuBuilder menuBuilder)
        {
            this.db = db;
            this.ytDLP = ytDLP;
            this.menuBuilder = menuBuilder;
        }

        #region Component Interaction
        [ComponentInteraction("SongAdd:*")]
        public async Task SongAdd(string videoAbsolutUri, string[] values)
        {
            string[] args = values[0].Split(",", StringSplitOptions.TrimEntries);
            uint playlistID = uint.Parse(args[0]);
            string playlistName = args[1];

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = "Adding a Song for you ♪. Please Wait...";
                o.Components = null;
            });

            if (!await ytDLP.DownloadMetaData(videoAbsolutUri, Context.Guild.Id))
            {
                await Context.Interaction.ModifyOriginalResponseAsync(o => {
                    o.Content = "Not a valid Song.";
                    o.Components = null;
                    });
                return;
            }

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

            await Context.Interaction.ModifyOriginalResponseAsync(o =>
            {
                o.Content = $"Song `{songInfo.Title}` added to playlist `{playlistName}`";
                o.Components = null;
            });
        }

        [ComponentInteraction("SongDeleteSelectPlaylist:*")]
        public async Task SongDeleteSelectPlaylist(uint page, string[] values)
        {
            var args = values[0].Split(',', StringSplitOptions.TrimEntries);
            var playlistID = uint.Parse(args[0]);

            var playlistSongs = db.Playlists.Include(s => s.Songs).First(pl => pl.ID == playlistID).Songs.Count;

            if(playlistSongs == 0)
            {
                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = "This Playlist has no Songs.";
                    o.Components = null;
                });
                return;
            }

            await ResponseWithDeleteSongInteraction(playlistID, page, customContent: "Select a Song by Number or move between pages." );
        }

        [ComponentInteraction("SongDeleteNumberButtonPressed:*,*,*")]
        public async Task SongDeleteSelectSong(uint playlistID, uint songID, uint page)
        {
            await ResponseWithDeleteSongInteraction(playlistID, page, songID);
        }

        [ComponentInteraction("SongDeletePageButtonPressed:*,*,*")]
        public async Task SongDeletePageButtonPressed(uint playlistID, uint page)
        {
            await ResponseWithDeleteSongInteraction(playlistID, page);
        }

        [ComponentInteraction("SongDeleteButtonDelete:*,*,*,*")]
        public async Task SongDeleteButtonDelete(uint playlistID, uint songID, uint page, uint playlistSongSize)
        {
            var playlist = db.Playlists
                .Include(s => s.Songs)
                .First(pl => pl.ID == playlistID);

            var song = playlist.Songs.First(s => s.ID == songID);
            var songTitle = song.Title;

            playlist.Songs.Remove(song);
            await db.SaveChangesAsync();

            page = playlist.Songs.Count >= playlistSongSize * (page - 1) + 1 ? page : page - 1;
            if(page == 0)
            {
                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = "No more Songs in this Playlist.";
                    o.Embed = null;
                    o.Components = null;
                });
                return;
            }

            await ResponseWithDeleteSongInteraction(playlistID, page, customContent: $"Song: {songTitle} deleted.");
        }

        [ComponentInteraction("SongDeleteButtonCancle")]
        public async Task SongDeleteButtonCancle()
        {
            await DeferAsync();
            await Context.Interaction.DeleteOriginalResponseAsync();
        }
        #endregion

        #region HelperFunctions

        private async Task ResponseWithDeleteSongInteraction(uint playlistID, uint page, uint? songID = null, string? customContent = null)
        {
            var playlist = db.Playlists.Include(s => s.Songs).First(pl => pl.ID == playlistID);
            uint playlistSongSize = 5; // can't me more then 5!

            //Embed
            var songsEmbed = menuBuilder.PlaylistSongsEmbedSelection(playlist, page, playlistSongSize);
            //Components
            var songNumbersButtons = menuBuilder.SongDeleteSongNumberButtons("SongDeleteNumberButtonPressed", playlist, page, playlistSongSize, songID);
            var playlistPageButtons = menuBuilder.PlaylistPageButtons("SongDeletePageButtonPressed", page, playlist, playlistSongSize);
            var cancelDeleteButtons = menuBuilder.CancelDeleteButtons("SongDeleteButtonCancle", "SongDeleteButtonDelete", page, playlistSongSize, playlistID, songID);

            // Components into Action Rows in right order
            var actionRowsList = new List<ActionRowBuilder>
            {
                songNumbersButtons,
                playlistPageButtons,
                cancelDeleteButtons
            };

            // Build all together
            var builder = new ComponentBuilder().WithRows(actionRowsList);

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = customContent;
                o.Embed = songsEmbed.Build();
                o.Components = builder.Build();
            });
        }

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
