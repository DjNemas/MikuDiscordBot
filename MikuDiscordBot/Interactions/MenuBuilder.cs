using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.Interactions
{
    public class MenuBuilder
    {
        private readonly DiscordDBContext db;

        public MenuBuilder(DiscordDBContext db) 
        {
            this.db = db;
        }

        /// <summary>
        /// <para>Build Cancle and Delete Button for SongDelete</para>
        /// <para>Sends customIDCancl and customIDDelete:playlistID,page,playlistSongSize</para>
        /// </summary>
        /// <param name="customIDCancel"></param>
        /// <param name="customIDDelete"></param>
        /// <param name="page"></param>
        /// <param name="playlistSongSize"></param>
        /// <param name="playlistID"></param>
        /// <param name="songID"></param>
        /// <returns></returns>
        public ActionRowBuilder CancelDeleteButtons(string customIDCancel, string customIDDelete, uint page,
            uint playlistSongSize, uint playlistID, uint? songID = null)
        {
            var cancelButton = new ButtonBuilder();
            cancelButton.WithCustomId($"{customIDCancel}");
            cancelButton.WithLabel("Cancel");
            cancelButton.WithStyle(ButtonStyle.Secondary);

            var deleteButton = new ButtonBuilder();
            deleteButton.WithCustomId($"{customIDDelete}:{playlistID},{songID},{page},{playlistSongSize}");
            deleteButton.WithLabel("Delete");
            deleteButton.WithStyle(ButtonStyle.Danger);
            if(songID is null)
                deleteButton.WithDisabled(true);
            else
                deleteButton.WithDisabled(false);

            return new ActionRowBuilder().WithButton(cancelButton).WithButton(deleteButton);
        }

        /// <summary>
        /// <para>Build All Buttons for Listed Delete Songs</para>
        /// <para>Sends customID:playlistID,songID,page</para>
        /// </summary>
        /// <param name="customID"></param>
        /// <param name="playlist"></param>
        /// <param name="page"></param>
        /// <param name="playlistSongSize"></param>
        /// <param name="selectedSongID"></param>
        /// <returns></returns>
        public ActionRowBuilder SongDeleteSongNumberButtons(string customID, Playlist playlist,
            uint page, uint playlistSongSize, uint? selectedSongID = null)
        {
            uint pageBeginNumber = playlistSongSize * (page - 1) + 1;

            uint pageEndNumber;
            if (playlist.Songs.Count <= playlistSongSize * page)
                pageEndNumber = (uint)playlist.Songs.Count;
            else
                pageEndNumber = playlistSongSize * page;

            var builder = new ActionRowBuilder();
            for (; pageBeginNumber <= pageEndNumber; pageBeginNumber++)
            {
                uint playlistID = playlist.ID;
                uint songID = playlist.Songs.ElementAt((int)(pageBeginNumber - 1)).ID;


                var numberButton = new ButtonBuilder();
                numberButton.WithCustomId($"{customID}:{playlistID},{songID},{page}");
                numberButton.WithDisabled(false);
                numberButton.WithLabel(pageBeginNumber.ToString());
                if(selectedSongID is not null && songID == selectedSongID)
                    numberButton.WithStyle(ButtonStyle.Danger);
                else
                    numberButton.WithStyle(ButtonStyle.Primary);
                builder.WithButton(numberButton);
            }
            return builder;
        }

        /// <summary>
        /// <para>Build Page Button for a Playlist</para>
        /// <para>Sends customID:playlistID,page,playlistSongSize</para>
        /// </summary>
        /// <param name="customID"></param>
        /// <param name="currentPage"></param>
        /// <param name="playlist"></param>
        /// <param name="playlistSongSize"></param>
        /// <returns></returns>
        public ActionRowBuilder PlaylistPageButtons(string customID, uint currentPage,
            Playlist playlist, uint playlistSongSize)
        {
            bool hasNextPage = playlist.Songs.Count > playlistSongSize * currentPage;

            var pageLeft = new ButtonBuilder()
            .WithCustomId($"{customID}:{playlist.ID},{currentPage - 1},{playlistSongSize}")
            .WithEmote(new Emoji("\u2B05"))
            .WithDisabled(currentPage == 1)
            .WithStyle(ButtonStyle.Primary);

            var current = new ButtonBuilder()
            .WithLabel($"Page {currentPage}")
            .WithCustomId("CurrentPage")
            .WithDisabled(true)
            .WithStyle(ButtonStyle.Secondary);

            var pageRight = new ButtonBuilder()
            .WithCustomId($"{customID}:{playlist.ID},{currentPage + 1},{playlistSongSize}")
            .WithEmote(new Emoji("\u27A1"))
            .WithDisabled(!hasNextPage)
            .WithStyle(ButtonStyle.Primary);

            var component = new ActionRowBuilder().WithButton(pageLeft).WithButton(current).WithButton(pageRight);

            return component;
        }

        /// <summary>
        /// <para>Build Playlist confirme delete Buttons</para>
        /// <para>Sends customIDYes:playlistID and customIDNo </para>
        /// </summary>
        /// <param name="customIDYes"></param>
        /// <param name="customIDNo"></param>
        /// <param name="playlistID"></param>
        /// <returns></returns>
        public ComponentBuilder PlaylistDeleteButtons(string customIDYes, string customIDNo, uint playlistID)
        {
            var button1 = new ButtonBuilder();
            button1.WithLabel("Yes");
            button1.WithStyle(ButtonStyle.Success);
            button1.WithCustomId($"{customIDYes}:{playlistID}");

            var button2 = new ButtonBuilder();
            button2.WithLabel("No");
            button2.WithStyle(ButtonStyle.Danger);
            button2.WithCustomId($"{customIDNo}");

            var builder = new ComponentBuilder();
            builder.WithButton(button1);
            builder.WithButton(button2);
            return builder;
        }

        public EmbedBuilder PlaylistSongsEmbedSelection(Playlist playlist, uint page, uint playlistSongSize)
        {
            uint songBeginNumber = 1 + playlistSongSize * (page - 1);
            uint songLastNumber = playlistSongSize * page;

            // if playlist contains more song that fit into this page ? use the maxpossible number for this page : else the last song
            uint pageEndNumber = playlist.Songs.Count > songLastNumber ? songLastNumber : (uint)playlist.Songs.Count;

            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Songs in playlist {playlist.PlaylistName}",
                Color = new Color(0x00ffff)
            };
            // Start Loop for Page SongNumber e.g. 26 - 50
            for (uint songNumber = songBeginNumber; songNumber <= pageEndNumber; songNumber++)
            {
                string SongTitle = playlist.Songs.ElementAt((int)songNumber - 1).Title;
                string VideoURL = playlist.Songs.ElementAt((int)songNumber - 1).VideoURL;
                embedBuilder.AddField($" Song {songNumber}",
                    $"[{SongTitle}]({VideoURL})");
            }
            return embedBuilder;
        }

        public SelectMenuBuilder PlaylistSelect(string customID, ulong guildID, uint? defaultPlaylistID = null)
        {
            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithCustomId(customID);
            menuBuilder.WithPlaceholder("Select a Playlist");
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            var playlists = db.GuildInfo.Include(pl => pl.Playlists)
                    .First(guild => guild.GuildID == guildID)
                    .Playlists;

            foreach (var pl in playlists)
            {
                string values = $"{pl.ID},{pl.PlaylistName}";
                if(defaultPlaylistID is not null && pl.ID == defaultPlaylistID)
                    menuBuilder.AddOption(pl.PlaylistName, values, isDefault: true);
                else
                    menuBuilder.AddOption(pl.PlaylistName, values);
            }
            return menuBuilder;
        }
    }
}
