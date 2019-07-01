using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using VideoServer.Shared;

namespace VideoServer.Server.Services
{
    public interface IVideoService : IDisposable
    {
        Task<Stream> ReadToStream(string filePath, float start, float duration);

        Task<FileStream> GetThumbnail(string filePath, float timestamp);
    }

    public class VideoService : IVideoService
    {
        private static string FFMpeg => Environment.OSVersion.Platform == PlatformID.Win32NT ? "ffmpeg.exe" : "ffmpeg";

        private List<IDisposable> disposeables = new List<IDisposable>();

        private IConfiguration _config;

        public VideoService(IConfiguration config) {
            _config = config;
        }

        public async Task<Stream> ReadToStream(string filePath, float start, float duration)
        {
            var id = Guid.NewGuid();
            var path = $"/{id}";
            var memoryFile = MemoryMappedFile.CreateNew(path, 1024 * 128, MemoryMappedFileAccess.ReadWrite);
            disposeables.Add(memoryFile);
            using (var p = new Process())
            {
                var startS = start.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                var durationS = duration.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                p.StartInfo.FileName = FFMpeg;
                p.StartInfo.Arguments = $"-v quiet -i {filePath} -c:v libx264 -ss {startS} -t {durationS} -f mp4 -y {path}";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                await Task.Run(() => p.WaitForExit());
            }
            var result = new FileStream(path, FileMode.Open);
            disposeables.Add(result);
            return result;
        }

        public async Task<FileStream> GetThumbnail(string filePath, float timestamp)
        {
            var cacheFolder = _config[SettingKeys.CACHE_FOLDER];
            string cachePath = $"{cacheFolder}/{filePath.ToSHA256()}_{timestamp}.jpg";

            if (!File.Exists(cachePath))
            {
                using (var p = new Process())
                {
                    var startS = timestamp.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    p.StartInfo.FileName = FFMpeg;
                    p.StartInfo.Arguments = $"-ss {startS} -v quiet -i \"{filePath}\" -an -vframes 1 -f image2pipe \"{cachePath}\"";
                    Console.WriteLine(p.StartInfo.Arguments);
                    p.Start();
                    
                    await Task.Run(() => p.WaitForExit());
                }
            }
            var result = new FileStream(cachePath, FileMode.Open);
            disposeables.Add(result);
            return result;
        }

        public void Dispose()
        {
            foreach(var disposable in disposeables) {
                disposable?.Dispose();
            }
        }
    }
}
