using Discord;
using Microsoft.EntityFrameworkCore;
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

        public ComponentBuilder PlaylistPageButtons(bool isPageOne, int currentPage, bool hasNextPage, uint playlistID)
        {
            var pageLeft = new ButtonBuilder()
            .WithCustomId($"PlaylistPageLeft,{currentPage - 1},{playlistID}")
            .WithEmote(new Emoji("\u2B05"))
            .WithDisabled(isPageOne)
            .WithStyle(ButtonStyle.Primary);

            var current = new ButtonBuilder()
            .WithLabel($"Page {currentPage}")
            .WithCustomId("CurrentPage")
            .WithDisabled(true)
            .WithStyle(ButtonStyle.Secondary);

            var pageRight = new ButtonBuilder()
            .WithCustomId($"PlaylistPageRight,{currentPage + 1},{playlistID}")
            .WithEmote(new Emoji("\u27A1"))
            .WithDisabled(!hasNextPage)
            .WithStyle(ButtonStyle.Primary);

            return new ComponentBuilder().WithButton(pageLeft).WithButton(current).WithButton(pageRight);
            
        }

        public ComponentBuilder PlaylistDeleteButtons(int playlistID)
        {
            var button1 = new ButtonBuilder();
            button1.WithLabel("Yes");
            button1.WithStyle(ButtonStyle.Success);
            button1.WithCustomId($"PlaylistDeleteYes,{playlistID}");

            var button2 = new ButtonBuilder();
            button2.WithLabel("No");
            button2.WithStyle(ButtonStyle.Danger);
            button2.WithCustomId("PlaylistDeleteNo");

            var builder = new ComponentBuilder();
            builder.WithButton(button1);
            builder.WithButton(button2);
            return builder;
        }

        public EmbedBuilder PlaylistSongsEmbedSelection(Playlist playlist, int page, int playlistSongSize)
        {
            int songBeginNumber = 1 + playlistSongSize * (page - 1);
            int songLastNumber = playlistSongSize * page;

            // if playlist contains more song that fit into this page ? use the maxpossible number for this page : else the last song
            int pageEndNumber = playlist.Songs.Count > songLastNumber ? songLastNumber : playlist.Songs.Count;

            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Songs in playlist {playlist.PlaylistName}",
                Color = new Color(0x00ffff)
            };
            // Start Loop for Page SongNumber e.g. 26 - 50
            for (int songNumber = songBeginNumber; songNumber <= pageEndNumber; songNumber++)
            {
                string SongTitle = playlist.Songs.ElementAt(songNumber - 1).Title;
                string VideoURL = playlist.Songs.ElementAt(songNumber - 1).VideoURL;
                embedBuilder.AddField($" Song {songNumber}",
                    $"[{SongTitle}]({VideoURL})");
            }
            return embedBuilder;
        }

        public SelectMenuBuilder PlaylistSelect(
            string customID,
            ulong guildID,
            bool withDefault = false,
            bool withNoneSelection = false,
            params string[] extraData)
        {
            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithCustomId(customID);
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            var playlists = db.GuildInfo.Include(pl => pl.Playlists)
                    .First(guild => guild.GuildID == guildID)
                    .Playlists;

            if(withNoneSelection)
                menuBuilder.AddOption("None", "0");

            StringBuilder sb = new StringBuilder();
            foreach (var data in extraData) { sb.Append(',').Append(data); }

            foreach (var pl in playlists)
            {
                string values = $"{pl.ID},{pl.PlaylistName}{sb}";
                if (withDefault && pl.ID == PlaylistManager.GuildPlaylist[guildID]?.selectedPlaylist?.ID)
                    menuBuilder.AddOption(pl.PlaylistName, values, isDefault: true);
                else
                    menuBuilder.AddOption(pl.PlaylistName, values);
            }
            return menuBuilder;
        }
    }
}
