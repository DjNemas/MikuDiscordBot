using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.Models
{
    public class ChatGPTResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("greeting_message")]
        public string? GreetingMessage { get; set; }
        [JsonPropertyName("data")]
        public string? Data { get; set; }
        [JsonPropertyName("log")]
        public bool Log { get; set; }
    }
}
