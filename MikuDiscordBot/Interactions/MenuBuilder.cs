using Discord;
using Microsoft.EntityFrameworkCore;
using MikuDiscordBot.Database;
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

        public SelectMenuBuilder BuildPlaylistSelection(string customID,
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
