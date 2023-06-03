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
    public class Song
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        public string VideoID { get; set; } = string.Empty;
        public string VideoURL { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public uint PlaylistID { get; set; }

    }
}
