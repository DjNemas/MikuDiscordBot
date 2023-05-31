using Discord.Audio;
using NAudio.Wave;

namespace MikuDiscordBot.MikuDiscord.MusicEngine
{
    public class MusicSystem
    {
        public static readonly Dictionary<ulong, MusicSystem> MusicSystemList = new();
        private List<MemoryStream> mp3Streams;
        private AudioOutStream? audioOutStream;
        public bool isPlaying { get; private set; } = false;
        private Mp3FileReader? currentSong;
        private CancellationTokenSource? copyCancellationToken;

        public MusicSystem(ulong guildID)
        {
            mp3Streams = new List<MemoryStream>();

            if(!MusicSystemList.ContainsKey(guildID))
                MusicSystemList.Add(guildID, this);
        }

        public async void Play()
        {
            if (isPlaying) return;
            if (!HasSongs()) return;

            if (audioOutStream is null) return;

            isPlaying = true;

            mp3Streams.First().Seek(0, SeekOrigin.Begin);

            // not allow to reposition the offset
            // Thats why data have to be stored in MemorySteam to reposition.
            using (currentSong = new Mp3FileReader(mp3Streams.First()))
            {
                copyCancellationToken = new CancellationTokenSource();
                try
                {
                    await currentSong.CopyToAsync(audioOutStream, copyCancellationToken.Token);
                }
                catch (Exception)
                {
                    // To avoid crash when music stops
                }
                finally
                {
                    await audioOutStream.FlushAsync();
                }

            }
            isPlaying = false;
        }

        public async void Stop()
        {
            copyCancellationToken?.Cancel();
            await Task.Delay(10);
        }

        public void AddSong(Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            mp3Streams.Add(ms);

            stream.Close();
        }

        public static void AddSong(ulong guildID, Stream stream)
        {
            MusicSystemList[guildID].AddSong(stream);
        }

        public void Joined(IAudioClient client)
        {
            audioOutStream = client.CreatePCMStream(AudioApplication.Mixed);
        }

        public void Disconnected()
        {
            Stop();
            isPlaying = false;
        }

        public bool HasSongs() => mp3Streams.Count > 0;

        public static void RemoveFromList(ulong guildID)
        {
            MusicSystemList[guildID].audioOutStream?.Dispose();
            MusicSystemList.Remove(guildID);
        }

    }
}
