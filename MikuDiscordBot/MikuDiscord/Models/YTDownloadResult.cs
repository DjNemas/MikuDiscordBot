using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.Models
{
    public class YTDownloadResult
    {
        public MemoryStream MP3Stream { get; set; } = new();
        public SongJsonInfo? SongJsonInfo { get; set; } = new();
    }
}
