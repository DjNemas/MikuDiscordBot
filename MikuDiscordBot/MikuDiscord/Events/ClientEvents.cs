﻿using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MikuDiscordBot.Database;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                await interaction.RegisterCommandsToGuildAsync(guild.Id, true);
                await Console.Out.WriteLineAsync("Commands Registred to Guild: " + guild.Name);
            }
            catch (Exception ex) 
            {
                await Console.Out.WriteLineAsync("Error on command registration.\n" + ex);
            }

            new MusicSystem(guild.Id);
            await new PlaylistManager(db).LoadGuildPlaylist(guild.Id);
        }

        public async Task SelectMenuExecuted(SocketMessageComponent arg)
        {
            uint id = uint.Parse(arg.Data.Value);
            if(arg.GuildId is not null)
                await PlaylistManager.GuildPlaylist[(ulong)arg.GuildId].ChangePlaylist(id);
            await arg.RespondAsync($"Playlist Selected: {arg.Data.Value}");
        }
    }
}
