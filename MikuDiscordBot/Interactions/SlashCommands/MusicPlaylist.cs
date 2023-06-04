using Discord;
using Discord.Interactions;
using MikuDiscordBot.Database.Models;
using MikuDiscordBot.Database;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;

namespace MikuDiscordBot.Interactions.SlashCommands
{
    public partial class Music : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [Group("playlist", "Manage Playlists")]
        public class MusicPlaylist : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
        {
            private readonly DiscordDBContext db;
            private readonly MenuBuilder menuBuilder;

            public MusicPlaylist(DiscordDBContext db, MenuBuilder menuBuilder)
            {
                this.db = db;
                this.menuBuilder = menuBuilder;
            }

            #region SlashCommands
            [SlashCommand("add", "Add a Playlist")]
            public async Task Add([MinLength(3), MaxLength(20)] string playlistName)
            {
                GuildInfo guild = await db.GuildInfo
                    .Include(pls => pls.Playlists)
                    .FirstAsync(guild => guild.GuildID == Context.Guild.Id);
                guild.Playlists.Add(new Playlist() { PlaylistName = playlistName });
                await db.SaveChangesAsync();
                await RespondAsync($"Playlist {playlistName} added.");
            }

            [SlashCommand("select", "Select a Playlist to use for playing Music.")]
            public async Task Select()
            {
                var menu = menuBuilder.PlaylistSelect("PlaylistSelect", Context.Guild.Id);
                var builder = new ComponentBuilder().WithSelectMenu(menu);

                await RespondAsync("Select a Playlist", components: builder.Build());
            }

            [SlashCommand("delete", "Deletes a Playlist.")]
            public async Task Delete()
            {
                var menu = menuBuilder.PlaylistSelect("PlaylistDelete", Context.Guild.Id);
                var builder = new ComponentBuilder().WithSelectMenu(menu);

                await RespondAsync("Which playlist do you want to delete?", components: builder.Build());
            }

            [SlashCommand("songs", "List Songs from Playlist.")]
            public async Task Songs([MinValue(1)]uint page = 1)
            {
                var menu = menuBuilder.PlaylistSelect($"PlaylistSongs:{page}", Context.Guild.Id);
                var builder = new ComponentBuilder().WithSelectMenu(menu);
            
                await RespondAsync("From which playlist do you want to see the Songs?", components: builder.Build());   
            }
            #endregion
        }

    }
}
