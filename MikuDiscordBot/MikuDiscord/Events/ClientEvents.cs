using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MikuDiscordBot.Database;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.FilesManager;
using MikuDiscordBot.Interactions;
using MikuDiscordBot.MikuDiscord.Models;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider service;

        public ClientEvents(DiscordDBContext db, InteractionService interaction, DiscordSocketClient client, IServiceProvider service)
        {
            this.db = db;
            this.interaction = interaction;
            this.client = client;
            this.service = service;
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

        public async Task SlashCommandExecuted(SocketSlashCommand component)
        {
            var ctx = new SocketInteractionContext<SocketSlashCommand>(client, component);
            await interaction.ExecuteCommandAsync(ctx, service);
        }

        public async Task ButtonExecuted(SocketMessageComponent component)
        {
            var ctx = new SocketInteractionContext<SocketMessageComponent>(client, component);
            await interaction.ExecuteCommandAsync(ctx, service);
        }

        public async Task SelectMenuExecuted(SocketMessageComponent component)
        {
            var ctx = new SocketInteractionContext<SocketMessageComponent>(client, component);
            await interaction.ExecuteCommandAsync(ctx, service);
        }
    }
}
