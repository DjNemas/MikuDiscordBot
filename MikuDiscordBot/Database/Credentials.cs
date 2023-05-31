using MikuDiscordBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuDiscordBot.Database
{
    public class Credentials
    {
        public static void InsertTokenDB(string? token)
        {
            if (token == null) throw new ArgumentNullException("Token is null");

            var discordDB = new DiscordDBContext();
            var config = discordDB.DiscordApiConfigs.FirstOrDefault(e => e.ID == 1);
            if (config != null)
            {
                if (config.DiscordToken == token)
                    return;
                else
                    config.DiscordToken = token;
            }
            else
            {
                config = new Models.Credentials() { DiscordToken = token };
                discordDB.Add(config);
            }
            discordDB.SaveChanges();
        }
    }
}
