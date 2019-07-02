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

        private ICacheSettings settings;

        public VideoService(ICacheSettings settings) {
            this.settings = settings;
        }

        public async Task<Stream> ReadToStream(string filePath, float start, float duration)
        {
            CheckCacheSize();
            var cacheFile = Path.Join(settings.Folder, $"{filePath.ToSHA256()}_{start}_{duration}.mp4");
            cacheFile = cacheFile.Replace(',', '-');

            if (!File.Exists(cacheFile)) {
                Console.WriteLine(cacheFile);
                using (var p = new Process())
                {
                    var startS = start.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                    var durationS = duration.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    p.StartInfo.FileName = FFMpeg;
                    p.StartInfo.Arguments = $" -i {filePath} -c:v libx264 -ss {startS} -t {durationS} -f mp4 -y {cacheFile}";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();

                    await Task.Run(() => p.WaitForExit());
                }
            }
            var result = new FileStream(cacheFile, FileMode.Open);
            disposeables.Add(result);
            return result;
        }

        public async Task<FileStream> GetThumbnail(string filePath, float timestamp)
        {
            CheckCacheSize();
            string cachePath = $"{settings.Folder}/{filePath.ToSHA256()}_{timestamp}.jpg";

            if (!File.Exists(cachePath))
            {
                using (var p = new Process())
                {
                    var startS = timestamp.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    p.StartInfo.FileName = FFMpeg;
                    p.StartInfo.Arguments = $"-ss {startS} -i \"{filePath}\" -an -vframes 1 -f image2pipe \"{cachePath}\"";
                    p.Start();
                    
                    await Task.Run(() => p.WaitForExit());
                }
            }
            var result = new FileStream(cachePath, FileMode.Open);
            disposeables.Add(result);
            return result;
        }

        private void CheckCacheSize() {
            if (!Directory.Exists(settings.Folder)) {
                Directory.CreateDirectory(settings.Folder);
                return;
            }
            if (Directory.GetFiles(settings.Folder).Length > settings.Size) {
                var files = new DirectoryInfo(settings.Folder).GetFiles().OrderBy(x => x.LastAccessTime).ToList();
                var l = files.Count;
                int i = 0;
                while (l-i > settings.Size) {
                    File.Delete(files[i++].FullName);
                }
            }
        }

        public void Dispose()
        {
            foreach(var disposable in disposeables) {
                disposable?.Dispose();
            }
        }
    }
}
