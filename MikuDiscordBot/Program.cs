using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MikuDiscordBot.Database;
using MikuDiscordBot.FilesManager;
using MikuDiscordBot.FilesManager.Models;
using MikuDiscordBot.Interactions;
using MikuDiscordBot.Interactions.SlashCommands;
using MikuDiscordBot.MikuDiscord;
using MikuDiscordBot.MikuDiscord.Events;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using MikuDiscordBot.MikuDiscord.SpeekEngine;
using Newtonsoft.Json.Linq;

namespace MikuDiscordBot
{
    internal class Program
    {
        private readonly IServiceProvider? serviceProvider;
        private DiscordDBContext db;
        private DiscordClientService? discordClientService;
        private DiscordInteractionService? interactionService;

        public Program()
        {
            serviceProvider = CreateProvider();
            db = serviceProvider.GetRequiredService<DiscordDBContext>();
        }

        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            Init();
            discordClientService = serviceProvider?.GetRequiredService<DiscordClientService>();

            interactionService = serviceProvider?.GetService<DiscordInteractionService>();
            interactionService?.Start();

            if(discordClientService is not null)
                await discordClientService.Start();
        }

        private static IServiceProvider CreateProvider()
        {
            var collection = new ServiceCollection()
                .AddSingleton<DiscordClientService>()
                .AddSingleton(DiscordClientService.GetDiscordSocketConfig())
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<DiscordInteractionService>()
                .AddSingleton(DiscordInteractionService.GetInteractionServiceConfig())
                .AddSingleton<InteractionService>()
                .AddSingleton<ClientEvents>()
                .AddSingleton<DiscordLog>()
                .AddSingleton<MenuBuilder>()
                .AddScoped<PlaylistManager>()
                .AddScoped<YTDLP>()
                // more here
                .AddDbContext<DiscordDBContext>(contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient);
            return collection.BuildServiceProvider();
        }

        private void Init()
        {
            Files.EnsureAllFolderExist();
            Files.EnsureConfigFileExist();

            try
            {
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                File.AppendAllText(Files.logAbsolutPath, $"[Error] [DBEnsure Create] [{DateTime.Now}] {ex}");
            }            

            var token = Files.GetConfigFile()?.DiscordToken;
            Credentials.InsertTokenDB(token);
        }
    }
}