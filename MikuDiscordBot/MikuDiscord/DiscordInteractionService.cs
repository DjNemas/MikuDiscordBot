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
        private readonly InteractionService interaction;
        private readonly DiscordLog log;
        private readonly IServiceProvider serviceProvider;

        public DiscordInteractionService(InteractionService interaction, DiscordLog log, IServiceProvider serviceProvider)
        {
            this.interaction = interaction;
            this.log = log;
            this.serviceProvider = serviceProvider;
            
        }

        public async Task Start()
        {
            interaction.Log += log.InteractionLog;

            try
            {
                await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("[Error] could not AddModules\n" + ex);
            }
        }

        public static InteractionServiceConfig GetInteractionServiceConfig() => 
            new InteractionServiceConfig()
            {
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async
            };
    }
}
