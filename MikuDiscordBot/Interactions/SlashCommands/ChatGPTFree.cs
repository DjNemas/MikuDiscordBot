using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MikuDiscordBot.MikuDiscord.SpeekEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.Interactions.SlashCommands
{
    public class ChatGPTFree : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("speak", "Speak with Miku", runMode: RunMode.Async)]
        public async Task SpeakCommand(string msg)
        {
            SocketGuildUser? user = Context.User as SocketGuildUser;
            await RespondAsync((user?.Nickname ?? Context.User.Username) + " wrote:\n" + msg);
            await FollowupAsync("Miku is Thinking...");

            var gpt = MikuDiscord.SpeekEngine.ChatGPTFree.GetClientForGuild(Context.Guild.Id);
            string? response = await gpt.SendText(msg);

            await ReplyAsync("Miku Response:\n" + response);
        }
    }
}
