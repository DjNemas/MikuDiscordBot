using Discord;
using Discord.Interactions;
using MikuDiscordBot.MikuDiscord.MusicEngine;
using Discord.WebSocket;
using Discord.Audio;

namespace MikuDiscordBot.Interactions.SlashCommands
{
    public partial class Music : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
    {
        [Group("control", "Control the Music")]
        public class MusicControl : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
        {
            private MusicSystem? guildMusicSystem;
            public override void BeforeExecute(ICommandInfo command)
            {
                guildMusicSystem = MusicSystem.MusicSystemList[Context.Guild.Id];
            }

            #region SlashCommands
            //[SlashCommand("play", "Start playing the Music.")]
            //public async Task Play()
            //{
            //    if (!MikuIsInVoiceChannel())
            //    {
            //        await RespondAsync("Miku is not in a VoiceChannel");
            //        return;
            //    }
            //    if (guildMusicSystem is not null && !guildMusicSystem.HasSongs())
            //    {
            //        await RespondAsync("There are no Song in the Playlist");
            //        return;

            //    }
            //    if (guildMusicSystem is not null && guildMusicSystem.isPlaying)
            //    {
            //        await RespondAsync("Music is already playing.");
            //        return;
            //    }
            //    guildMusicSystem?.Play();
            //    await RespondAsync("*knock knock* miku Okay~ Music Start!");
            //}

            //[SlashCommand("join", "Miku joins the voice channel u are currently in.")]
            //public async Task Join()
            //{
            //    if (MikuIsInVoiceChannel())
            //    {
            //        await RespondAsync("Miku already joined a voice channel.");
            //        return;
            //    }

            //    SocketVoiceChannel? channel = (Context.User as SocketGuildUser)?.VoiceChannel;
            //    if (channel == null)
            //    {
            //        await RespondAsync("You must be in a voice channel.");
            //        return;
            //    }

            //    IAudioClient audioClient = await channel.ConnectAsync();
            //    guildMusicSystem?.Joined(audioClient);

            //    await RespondAsync("Joined");
            //    await DeleteOriginalResponseAsync();
            //}

            //[SlashCommand("leave", "Miku leaves the current Voice channel.")]
            //public async Task Leave()
            //{
            //    var voiceChannel = GetMikusCurrentVoiceChannel();
            //    if (voiceChannel is null)
            //    {
            //        await RespondAsync("Miku is currently not in a voice channel.");
            //        return;
            //    }
            //    await voiceChannel.DisconnectAsync();

            //    guildMusicSystem?.Disconnected();

            //    await RespondAsync("Disconnected");
            //    await DeleteOriginalResponseAsync();
            //}

            //[SlashCommand("stop", "Stops playing the Music.")]
            //public async Task Stop()
            //{
            //    if (!MikuIsInVoiceChannel())
            //    {
            //        await RespondAsync("Miku is not in a VoiceChannel.");
            //        return;
            //    }
            //    if (guildMusicSystem is not null && !guildMusicSystem.isPlaying)
            //    {
            //        await RespondAsync("Music is not Playing.");
            //        return;
            //    }
            //    guildMusicSystem?.Stop();
            //    await RespondAsync("Music Stopped.");
            //}
            #endregion

            #region HelperFunctions
            private bool MikuIsInVoiceChannel()
            {
                foreach (var ch in Context.Guild.Channels)
                {
                    if (ch.GetChannelType() == ChannelType.Voice)
                    {
                        var vch = ch as SocketVoiceChannel;

                        if (vch is not null) // -_- stupid check
                            foreach (var user in vch.ConnectedUsers)
                            {
                                if (user.Id == Context.Client.CurrentUser.Id)
                                    return true;
                            }
                    }
                }
                return false;
            }

            private SocketVoiceChannel? GetMikusCurrentVoiceChannel()
            {
                foreach (var ch in Context.Guild.Channels)
                {
                    if (ch.GetChannelType() == ChannelType.Voice)
                    {
                        var vch = ch as SocketVoiceChannel;

                        if (vch is not null) // -_- stupid check
                            foreach (var user in vch.ConnectedUsers)
                            {
                                if (user.Id == Context.Client.CurrentUser.Id)
                                    return ch as SocketVoiceChannel;
                            }
                    }
                }
                return null;
            }
            #endregion
        }

    }
}
