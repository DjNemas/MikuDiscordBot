using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.Database.Models
{
    [PrimaryKey("GuildID")]
    public class GuildInfo
    {
        [Key]
        public ulong GuildID { get; set; }
        [ForeignKey("GuildID")]
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
