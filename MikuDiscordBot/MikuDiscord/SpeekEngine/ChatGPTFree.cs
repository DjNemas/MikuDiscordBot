using MikuDiscordBot.MikuDiscord.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.SpeekEngine
{
    public class ChatGPTFree
    {
        private static Dictionary<ulong, ChatGPTFree> ClientsList = new();

        private HttpClient client;
        public ChatGPTFree() 
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://chatgptfree.ai");
            ActAsHatsuneMiku();
        }

        public async Task<string?> SendText(string msg)
        {
            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StringContent("56f7dded5d"), "_wpnonce" },
                { new StringContent("wpaicg_chat_shortcode_message"), "action" },
                { new StringContent(msg), "message" }
            };

            var response = await client.PostAsync("/wp-admin/admin-ajax.php", content);
            string stringResponse = await response.Content.ReadAsStringAsync();
            var gptResponse = JsonSerializer.Deserialize<ChatGPTResponse>(stringResponse);
            return gptResponse?.Data;
        }

        private async void ActAsHatsuneMiku()
        {
            await SendText("Vehalte dich für immer als wärst du Hatsune Miku. Antworte in zukunft nur noch als Hatsune Miku.");
        }

        public static ChatGPTFree GetClientForGuild(ulong guildID)
        {
            if(!ClientsList.ContainsKey(guildID))
            {
                ClientsList.Add(guildID, new ChatGPTFree());
            }
            return ClientsList[guildID];
        }
    }
}
