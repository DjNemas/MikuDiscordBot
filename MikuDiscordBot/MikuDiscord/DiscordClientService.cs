using Discord.WebSocket;
using Discord;
using MikuDiscordBot.Database;
using MikuDiscordBot.MikuDiscord.Events;
using Discord.Net.Queue;

namespace MikuDiscordBot.MikuDiscord
{
    public class DiscordClientService
    {
        private DiscordSocketClient client;
        private DiscordLog log;
        private DiscordDBContext db;
        private ClientEvents events;

        public DiscordClientService(DiscordSocketClient client, DiscordLog log, ClientEvents events, DiscordDBContext db)
        {
            this.client = client;
            this.log = log;
            this.db = db;
            this.events = events;
        }

        public async Task Start()
        {
            client.Log += log.ClientLog;
            client.GuildAvailable += events.GuildAvailable;
            client.SlashCommandExecuted += events.SlashCommandExecuted;
            client.SelectMenuExecuted += events.SelectMenuExecuted;
            client.ButtonExecuted += events.ButtonExecuted;

            await client.LoginAsync(TokenType.Bot, GetTokenFromDB());
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private string? GetTokenFromDB()
        {
            return db.DiscordApiConfigs.FirstOrDefault(config => config.ID == 1)?.DiscordToken;
        }

        public static DiscordSocketConfig GetDiscordSocketConfig() => new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
    }
}
