using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
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
        private static string FFProbe => Environment.OSVersion.Platform == PlatformID.Win32NT ? "ffprobe.exe" : "ffprobe";

        private List<IDisposable> disposeables = new List<IDisposable>();

        private ICacheSettings settings;

        private char[] charBuffer = new char[4096];

        public VideoService(ICacheSettings settings) {
            this.settings = settings;
        }

        private async Task<int> GetClosestKeyframe(string filePath, float start) {
            using (var p = new Process()) {
                var t = TimeSpan.FromSeconds(start-10);

                p.StartInfo.FileName = FFProbe;
                p.StartInfo.Arguments = $"-v quiet -select_streams v -show_frames -show_entries frame=pkt_pts,pkt_pts_time -skip_frame nokey -read_intervals {t.Hours}:{t.Minutes}:{t.Seconds}.{t.Milliseconds}%+20 -of csv -i {filePath}";
                Console.WriteLine(p.StartInfo.Arguments);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                var outputBuilder = new StringBuilder();
                int read = 1;
                while(read > 0) {
                    read = await p.StandardOutput.ReadAsync(charBuffer, 0, charBuffer.Length);
                    outputBuilder.Append(charBuffer.Take(read).ToArray());
                }
                var outputString = outputBuilder.ToString();
                Console.WriteLine(outputString);
                var results = outputString.Split('\n').Select(s => {
                    var parts = s.Split(',');
                    if (parts.Length < 3) {
                        return (-1, float.MaxValue);
                    }
                    return (int.Parse(parts[1]), float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat));
                });
                (int frame, float time) = results.OrderBy(a => Math.Abs(a.Item2-start)).First();
                return frame;
            }
        }

        public async Task<Stream> ReadToStream(string filePath, float start, float duration)
        {
            CheckCacheSize();
            var cacheFile = Path.Join(settings.Folder, $"{filePath.ToSHA256()}_{start}_{duration}.mp4");
            cacheFile = cacheFile.Replace(',', '-');

            if (!File.Exists(cacheFile)) {
                int startFrame = await GetClosestKeyframe(filePath, start);

                using (var p = new Process())
                {
                    var startS = start.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                    var durationS = duration.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    p.StartInfo.FileName = FFMpeg;
                    p.StartInfo.Arguments = $"-v quiet -i {filePath} -c copy -start_number {startFrame} -t {durationS} -f mp4 -y {cacheFile}";
                    Console.WriteLine(p.StartInfo.Arguments);
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
            Console.WriteLine(filePath);
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
