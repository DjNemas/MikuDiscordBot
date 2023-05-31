using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MikuDiscordBot.FilesManager.Models
{
    public class ConfigJson
    {
        [JsonPropertyName("discordToken")]
        public string DiscordToken { get; set; } = "Your Token Here";
    }
}
