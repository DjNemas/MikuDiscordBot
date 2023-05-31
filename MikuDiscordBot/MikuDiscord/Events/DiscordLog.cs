using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.Events
{
    public class DiscordLog
    {
        public async Task ClientLog(LogMessage msg)
        {
            await Console.Out.WriteLineAsync("[Client]" + msg.ToString());
        }

        public async Task InteractionLog(LogMessage msg)
        {
            await Console.Out.WriteLineAsync("[Interaction]" + msg.ToString());
        }
    }
}
