using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VideoServer.Server
{
    public class VideoStream : IDisposable
    {
        private byte[] buffer;

        public string filePath;
        public float start;
        public float duration;

        private List<IDisposable> disposeables = new List<IDisposable>();

        public VideoStream(string filePath, float start, float duration, int bufferSize=4096)
        {
            this.buffer = new byte[bufferSize];

            this.filePath = filePath;
            this.start = start;
            this.duration = duration;
        }

        public async Task<Stream> ReadToStream()
        {
            var id = Guid.NewGuid();
            var path = $"/{id}";
            var memoryFile = MemoryMappedFile.CreateNew(path, 1024 * 128, MemoryMappedFileAccess.ReadWrite);
            disposeables.Add(memoryFile);
            using (var p = new Process())
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    p.StartInfo.FileName = "ffmpeg.exe";
                }
                else
                {
                    p.StartInfo.FileName = "ffmpeg";
                }

                var startS = start.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                var durationS = duration.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

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

        public async Task<FileStream> GetThumbnail()
        {
            Directory.CreateDirectory(".cache");
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(filePath));
            var builder = new StringBuilder();
            foreach (var bytes in hash)
                builder.Append(bytes.ToString("x2"));

            string cachePath = $".cache/{builder.ToString()}_{start}.jpg";

            if (!File.Exists(cachePath))
            {
                using (var p = new Process())
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        p.StartInfo.FileName = "ffmpeg.exe";
                    }
                    else
                    {
                        p.StartInfo.FileName = "ffmpeg";
                    }

                    var startS = start.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

                    p.StartInfo.Arguments = $"-ss {startS} -v quiet -i \"{filePath}\" -an -vframes 1 -f image2pipe \"{cachePath}\"";
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
