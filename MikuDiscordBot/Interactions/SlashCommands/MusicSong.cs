using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MikuDiscordBot.MikuDiscord.MusicEngine;

namespace MikuDiscordBot.Interactions.SlashCommands
{
    [Group("music", "Miku can play Musik for you!")]
    public partial class Music : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [Group("song", "Manage Songs")]
        public class MusicSong : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
        {
            
            private readonly MenuBuilder menuBuilder;
            public MusicSong(MenuBuilder menuBuilder)
            {
                this.menuBuilder = menuBuilder;
            }
            #region SlashCommands
            [SlashCommand("add", "Add a Song to Playlist. Allowed: youtube.com/de, music.youtube.com, with watch?v={id}")]
            public async Task SongAdd(string youTubeUrl)
            {
                Uri? uri = await CheckYTUrlIsCorrect(youTubeUrl);
                if (uri is null) return;

                var menu = menuBuilder.PlaylistSelect($"SongAdd:{uri.AbsoluteUri}", Context.Guild.Id);
                var builder = new ComponentBuilder().WithSelectMenu(menu);

                await RespondAsync("Select the playlist to which you want to add the song.", components: builder.Build());          
            }

            [SlashCommand("delete", "Add a Song to Playlist. Allowed: youtube.com/de, music.youtube.com, with watch?v={id}")]
            public async Task SongDelete()
            {
                uint page = 1;
                var menuSelection = menuBuilder.PlaylistSelect($"SongDeleteSelectPlaylist:{page}", Context.Guild.Id);
                var builder = new ComponentBuilder().WithSelectMenu(menuSelection);

                await RespondAsync("From which Playlist do you want to delete a Song?", components: builder.Build());

            }
            #endregion

            #region HelperFunctions
            private async Task<Uri?> CheckYTUrlIsCorrect(string youTubeUrl)
            {
                // Add https:// if missing
                if (!youTubeUrl.StartsWith("https://"))
                    youTubeUrl = "https://" + youTubeUrl;

                Uri? uri = null;
                Uri.TryCreate(youTubeUrl, UriKind.Absolute, out uri);
                if (uri is null)
                {
                    await RespondAsync("Not a Valid URL.\n" +
                        "Allowed Domains: youtube.de, youtube.com, music.youtube.com\n" +
                        "Need to have a ID in URL: /watch?v={id}\n" +
                        "Example: https://www.youtube.com/watch?v=xIOg_K6Z1fg");
                    return null;
                }
                string host = uri.Host.StartsWith("www") ? uri.Host : "www." + uri.Host;
                if (host != "www.youtube.com" &&
                    host != "www.youtube.de" &&
                    host != "www.music.youtube.com")
                {
                    await RespondAsync("Not a Valid domain.\n" +
                        "Allowed Domains: youtube.de, youtube.com, music.youtube.com");
                    return null;
                }

                if (!youTubeUrl.Contains("watch?v="))
                {
                    await RespondAsync("Video ID missing.\n" +
                        "Please ensure the url contains a video id (watch?v={id})");
                    return null;
                }
                return uri;
            }
            #endregion
        }

    }
}
