using Discord.Interactions;
using Discord.WebSocket;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using MikuDiscordBot.MikuDiscord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using MikuDiscordBot.Database;
using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database.Models;
using Discord;
using System.Runtime.CompilerServices;

namespace MikuDiscordBot.Interactions.ComponentInteraction
{
    public class MusicPlaylist : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        private readonly MenuBuilder menuBuilder;
        private readonly DiscordDBContext db;

        public MusicPlaylist(MenuBuilder menuBuilder, DiscordDBContext db)
        {
            this.menuBuilder = menuBuilder;
            this.db = db;
        }
        #region ComponentInteraction
        [ComponentInteraction("PlaylistSelect", true)]
        public async Task PlaylistSelect(string[] values)
        {
            var args = values[0].Split(",", StringSplitOptions.TrimEntries);

            var playlistID = uint.Parse(args[0]);
            var playlistName = args[1];

            await PlaylistManager.GuildPlaylist[Context.Guild.Id].ChangePlaylist(playlistID);

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = $"Playlist Selected: {playlistName}";
                o.Components = null;
            });
        }

        [ComponentInteraction("PlaylistDelete", true)]
        public async Task PlaylistDelete(string[] values)
        {
            string[] args = values[0].Split(",", StringSplitOptions.TrimEntries);
            var playlistID = uint.Parse(args[0]);
            var playlistName = args[1];

            var builder = menuBuilder.PlaylistDeleteButtons("PlaylistDeleteYes", "PlaylistDeleteNo", playlistID);

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = $"Are your sure u want to Delete?\nPlaylist: {playlistName}";
                o.Components = builder.Build();
            });
        }

        [ComponentInteraction("PlaylistDeleteYes:*", true)]
        public async Task PlaylistDeleteYes(uint playlistID)
        {
            var guildInfo = db.GuildInfo
                .Include(pls => pls.Playlists)
                .ThenInclude(s => s.Songs)
                .First(guild => guild.GuildID == Context.Guild.Id);

            var playlist = guildInfo.Playlists.First(pl => pl.ID == playlistID);

            if (playlist.PlaylistName == "Default")
            {
                await DeleteDefaultPlaylist(playlist);
                return;
            }

            string playlistName = playlist.PlaylistName;

            var deleted = await DeletePlaylistDB(playlist);
            if (!deleted) return;

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = $"Playlist `{playlistName}` deleted.";
                o.Components = null;
            });
        }

        [ComponentInteraction("PlaylistDeleteNo", true)]
        public async Task PlaylistDeleteNo()
        {
            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = "Alright! No playlist deleted.";
                o.Components = null;
            });
        }

        [ComponentInteraction("PlaylistDeleteDefaultSongsYes:*", true)]
        public async Task PlaylistDeleteDefaultSongsYes(uint playlistID)
        {
            var playlist = db.Playlists.Include(s => s.Songs).First(pl => pl.ID == playlistID);

            var deleted = await DeletePlaylistDefaultSongsDB(playlist);
            if (!deleted) return;

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = $"Songs for Default Playlist deleted.";
                o.Components = null;
            });
        }

        [ComponentInteraction("PlaylistDeleteDefaultSongsNo", true)]
        public async Task PlaylistDeleteDefaultSongsNo()
        {
            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = "Alright! No songs deleted.";
                o.Components = null;
            });
        }

        [ComponentInteraction("PlaylistSongs:*", true)]
        public async Task PlaylistSongs(uint page, string[] values)
        {
            var args = values[0].Split(',', StringSplitOptions.TrimEntries);
            var playlistID = uint.Parse(args[0]);
            //var playlistName = args[1];
            uint playlistSongSize = 10;

            var playlist = await db.Playlists
                .Include(s => s.Songs)
                .FirstAsync(pl => pl.ID == playlistID);

            if (playlist.Songs.Count == 0)
            {
                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = "No Songs in this Playlist.";
                    o.Components = null;
                });
                return;
            }
            else if (!(playlist.Songs.Count > playlistSongSize * (page - 1)))
            {
                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = $"Page {page} doesn't exist for {playlist.PlaylistName}.";
                    o.Components = null;
                });
                return;
            }

            // Build PlaylistSong Respons Property
            (var embedBuilder, var buttonBuilder) = PlaylistSongBuilder("PlaylistPage", playlist, page, playlistSongSize);

            var listActionRow = new List<ActionRowBuilder>() { buttonBuilder };
            var componentBuilder = new ComponentBuilder().WithRows(listActionRow);

            await Context.Interaction.UpdateAsync(o =>
            {
                o.Content = null;
                o.Embed = embedBuilder.Build();
                o.Components = componentBuilder.Build();
            });
        }

        [ComponentInteraction("PlaylistPage:*,*,*", true)]
        public async Task PlaylistPage(uint playlistID, uint page, uint playlistSongSize)
        {
            var playlist = await db.Playlists
                .Include(s => s.Songs)
                .FirstAsync(pl => pl.ID == playlistID);

            (var embedBuilder, var buttonBuilder) = PlaylistSongBuilder("PlaylistPage", playlist, page, playlistSongSize);

            var listActionRow = new List<ActionRowBuilder>() { buttonBuilder };
            var componentBuilder = new ComponentBuilder().WithRows(listActionRow);

            await Context.Interaction.UpdateAsync(o => {
                o.Content = null;
                o.Embed = embedBuilder.Build();
                o.Components = componentBuilder.Build();
            });
        }
        #endregion

        #region Helper Functions

        // Tupel Power :D
        private (EmbedBuilder, ActionRowBuilder) PlaylistSongBuilder(string customIDButtons, Playlist playlist, uint page, uint playlistSongSize)
        {
            var embedBuilder = menuBuilder.PlaylistSongsEmbedSelection(playlist, page, playlistSongSize);

            var buttonBuilder = menuBuilder.PlaylistPageButtons(customIDButtons, page, playlist, playlistSongSize);

            return (embedBuilder, buttonBuilder);
        }

        private async Task DeleteDefaultPlaylist(Playlist playlist)
        {
            if (playlist.Songs.Count > 0)
            {
                var builder = menuBuilder.PlaylistDeleteButtons(
                    "PlaylistDeleteDefaultSongsYes",
                    "PlaylistDeleteDefaultSongsNo",
                    playlist.ID);

                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = $"Playlist `Default` can't be deleted but contains Songs.\n" +
                    "Do you want to delete all Songs from Default playlist?";
                    o.Components = builder.Build();
                });
                return;
            }
            else
            {
                await Context.Interaction.UpdateAsync(o =>
                {
                    o.Content = $"Playlist `Default` can't be deleted.";
                    o.Components = null;
                });
                return;
            }
        }

        private async Task<bool> DeletePlaylistDB(Playlist playlist)
        {
            try
            {
                db.RemoveRange(playlist.Songs);
                db.Remove(playlist);
                await db.SaveChangesAsync();
                PlaylistManager.GuildPlaylist[Context.Guild.Id].UpdateAllPlaylistForGuild();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] Could not delete Playlist.\n" + ex);
                await Errors.ReportErrorUpdate(102, Context.Interaction);
                return false;
            }
        }

        private async Task<bool> DeletePlaylistDefaultSongsDB(Playlist playlist)
        {
            try
            {
                db.RemoveRange(playlist.Songs);
                await db.SaveChangesAsync();
                PlaylistManager.GuildPlaylist[Context.Guild.Id].UpdateAllPlaylistForGuild();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] Could not delete Songs for Default Playlist.\n" + ex);
                await Errors.ReportErrorUpdate(103, Context.Interaction);
                return false;
            }
        }
        #endregion
    }
}
