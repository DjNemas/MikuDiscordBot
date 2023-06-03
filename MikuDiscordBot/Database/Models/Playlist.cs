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
    [PrimaryKey("ID")]
    public class Playlist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        public string PlaylistName { get; set; } = string.Empty;

        [ForeignKey("PlaylistID")]
        public List<Song> Songs { get; set; } = new List<Song>();
        public ulong GuildID { get; set; }

        public Playlist Clone()
        {
            return (Playlist)MemberwiseClone();
        }
    }
}
