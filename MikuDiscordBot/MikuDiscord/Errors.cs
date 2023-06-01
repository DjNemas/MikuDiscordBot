using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord
{
    public class Errors
    {
        public static async Task ReportErrorUpdate(short ErrorCode, SocketMessageComponent arg)
        {
            await arg.UpdateAsync(o =>
            {
                o.Content = $"[Error {ErrorCode}] Something went wrong.\n" +
                $"Please report a Issue with the given Error Code here: https://github.com/DjNemas/MikuDiscordBot/issues";
                o.Components = null;
            });
        }
    }
}
