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

        public async Task<bool> IsValidVideo(string url)
        {
            processStartInfo.Arguments = $"-s --config-location yt-dlp.conf {url}";
            await StartProcess();
            return isValidVideo;
        }

        public async Task<YTDownloadResult> DownloadMp3(string url, ulong guildID)
        {
            // Create Guild Folder
            var guildFolder = new DirectoryInfo(Path.Combine(Files.musicAbsolutPath, guildID.ToString()));
            Files.EnsureFolderExist(guildFolder);

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
                if (e.Data.Contains("Unsupported URL"))
                    isValidVideo = false;
            }
        }
#endif
    }
}
