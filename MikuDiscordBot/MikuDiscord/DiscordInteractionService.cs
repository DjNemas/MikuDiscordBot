using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MikuDiscordBot.Interactions.SlashCommands;
using MikuDiscordBot.MikuDiscord.Events;
using System.Reflection;

namespace MikuDiscordBot.MikuDiscord
{
    public class DiscordInteractionService
    {
        private readonly DiscordSocketClient client;
        private readonly InteractionService interaction;
        private readonly DiscordLog log;
        private readonly IServiceProvider serviceProvider;

        public DiscordInteractionService(DiscordSocketClient client, InteractionService interaction, DiscordLog log, IServiceProvider serviceProvider)
        {
            this.client = client;
            this.interaction = interaction;
            this.log = log;
            this.serviceProvider = serviceProvider;
        }

        public async Task Start()
        {
            interaction.Log += log.InteractionLog;
            client.InteractionCreated += Client_InteractionCreated;

            var modules = await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            var ctx = new SocketInteractionContext(client, arg);
            await interaction.ExecuteCommandAsync(ctx, serviceProvider);
        }

        public static InteractionServiceConfig GetInteractionServiceConfig()
        {
            return new InteractionServiceConfig()
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async
            };
        }
    }
}
