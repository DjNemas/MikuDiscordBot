using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;
using MikuDiscordBot.MikuDiscord;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MikuDiscordBot.Interactions.SlashCommands
{

    [Group("music", "Plays Music in Voice Chat")]
    public class Music : InteractionModuleBase<SocketInteractionContext>
    {
        private MusicSystem? guildMusicSystem;

        public override void BeforeExecute(ICommandInfo command)
        {
            guildMusicSystem = MusicSystem.MusicSystemList[Context.Guild.Id];
        }

        [SlashCommand("add", "Add a Song to the Playlist.")]
        public async Task Add()
        {
            FileStream file = new(@"C:\Users\denis\Desktop\775 WATER Feat. Miku - A-39【オリジナル曲】.mp3", FileMode.Open);
            guildMusicSystem?.AddSong(file);

            await RespondAsync("Song Added");
        }

        [SlashCommand("play", "Start playing the Music.")]
        public async Task Play()
        {
            if(!MikuIsInVoiceChannel())
            {
                await RespondAsync("Miku is not in a VoiceChannel");
                return;
            }
            if(guildMusicSystem is not null && !guildMusicSystem.HasSongs())
            {
                await RespondAsync("There are no Song in the Playlist");
                return;
                
            }
            if(guildMusicSystem is not null && guildMusicSystem.isPlaying)
            {
                await RespondAsync("Music is already playing.");
                return;
            }
            guildMusicSystem?.Play();
            await RespondAsync("*knock knock* miku Okay~ Music Start!");
        }

        [SlashCommand("join", "Miku joins the voice channel u are currently in.")]
        public async Task Join()
        {
            if(MikuIsInVoiceChannel())
            {
                await RespondAsync("Miku already joined a voice channel.");
                return;
            }

            SocketVoiceChannel? channel = (Context.User as SocketGuildUser)?.VoiceChannel;
            if (channel == null) 
            {
                await RespondAsync("You must be in a voice channel."); 
                return;
            }

            IAudioClient audioClient = await channel.ConnectAsync();
            guildMusicSystem?.Joined(audioClient);

            await RespondAsync("Joined");
            await DeleteOriginalResponseAsync();
        }

        [SlashCommand("leave", "Miku leaves the current Voice channel.")]
        public async Task Leave()
        {
            var voiceChannel = GetMikusCurrentVoiceChannel();
            if(voiceChannel is null)
            {
                await RespondAsync("Miku is currently not in a voice channel.");
                return;
            }
            await voiceChannel.DisconnectAsync();

            guildMusicSystem?.Disconnected();

            await RespondAsync("Disconnected");
            await DeleteOriginalResponseAsync();
        }

        [SlashCommand("stop", "Stops playing the Music.")]
        public async Task Stop()
        {
            if(!MikuIsInVoiceChannel())
            {
                await RespondAsync("Miku is not in a VoiceChannel.");
                return;
            }
            if(guildMusicSystem is not null && !guildMusicSystem.isPlaying)
            {
                await RespondAsync("Music is not Playing.");
                return;
            }
            guildMusicSystem?.Stop();
            await RespondAsync("Music Stopped.");
        }

        private bool MikuIsInVoiceChannel()
        {
            foreach (var ch in Context.Guild.Channels)
            {
                if (ch.GetChannelType() == ChannelType.Voice)
                {
                    var vch = ch as SocketVoiceChannel;

                    if(vch is not null) // -_- stupid check
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
            foreach(var ch in Context.Guild.Channels)
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
    }
}
