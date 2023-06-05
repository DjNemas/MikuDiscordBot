using MikuDiscordBot.FilesManager;
using MikuDiscordBot.MikuDiscord.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikuDiscordBot.MikuDiscord.MusicEngine
{
    public class YTDLP
    {
        private ProcessStartInfo processStartInfo;
        private bool isValidVideo = true;
        public YTDLP()
        {
            processStartInfo = new ProcessStartInfo()
            {
                FileName = "yt-dlp.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };
        }

        /// <summary>
        /// Downloads MetaData for Video
        /// </summary>
        /// <param name="url">The Video URL</param>
        /// <param name="guildID">Select for which GuildID it should be download.</param>
        /// <returns>Returns True if downloaded. False if Video is not Valid.</returns>
        public async Task<bool> DownloadMetaData(string url, ulong guildID)
        {
            var guildFolder = Files.EnsureMusicGuildFolderExist(guildID);

            processStartInfo.Arguments = $"--config-location yt-dlp.conf --skip-download -P \"{guildFolder.FullName}\" {url}";
            await StartProcess();

            return isValidVideo;
        }

        public async Task<YTDownloadResult> DownloadMp3(string url, ulong guildID)
        {
            var guildFolder = Files.EnsureMusicGuildFolderExist(guildID);

            // Start YT-DLP
            processStartInfo.Arguments = $"--config-location yt-dlp.conf -P \"{guildFolder.FullName}\" {url}";
            await StartProcess();

            //Prepare Result
            var result = new YTDownloadResult();

            // Add mp3 to result
            using (var mp3 = new FileStream(Path.Combine(guildFolder.FullName, "music.mp3"), FileMode.Open))
                await mp3.CopyToAsync(result.MP3Stream);

            // Add Metadata to Result
            result.SongJsonInfo = JsonSerializer.Deserialize<SongJsonInfo>(
                File.ReadAllText(Path.Combine(guildFolder.FullName, "music.info.json")));

            // Delete all Files 
            Files.DeleteAllInDir(guildFolder);

            return result;
        }

        private Task StartProcess()
        {
            var process = new Process();
            process.StartInfo = processStartInfo;
#if DEBUG
            process.ErrorDataReceived += Ytdl_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
#endif
            process.Start();
#if DEBUG
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
#endif
            process.WaitForExit();
            return Task.CompletedTask;
        }
#if DEBUG
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data is not null)
            {
                Console.WriteLine("[YTDLP Log] " + e.Data);
            }
        }

        private void Ytdl_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
            {
                Console.WriteLine("[YTDLP Error] " + e.Data);
                if (e.Data.Contains("Unsupported URL") || e.Data.Contains("is not a valid URL"))
                    isValidVideo = false;
            }
        }
#endif
    }
}
