using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.Models
{
    public class SongJsonInfo
    {
        [JsonPropertyName("id")]
        public string VideoID { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("webpage_url")]
        public string VideoUrl { get; set; } = string.Empty;
    }
}
