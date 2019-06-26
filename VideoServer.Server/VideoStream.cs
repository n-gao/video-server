using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VideoServer.Server
{
    public class VideoStream
    {
        private byte[] buffer;

        public string filePath;
        public float start;
        public float duration;

        public VideoStream(string filePath, float start, float duration, int bufferSize=4096)
        {
            this.buffer = new byte[bufferSize];

            this.filePath = filePath;
            this.start = start;
            this.duration = duration;
        }

        public async Task<MemoryStream> ReadToStream()
        {
            var result = new MemoryStream();
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

                //p.StartInfo.Arguments = $"-v quiet -i {filePath} -c:v libx264 -ss {startS} -t {durationS} -f matroska pipe:";
                p.StartInfo.Arguments = $"-v quiet -i {filePath} -c copy -ss {startS} -t {durationS} -f matroska pipe:";
                Console.Write(p.StartInfo.Arguments);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                int read = 1;
                while (read > 0)
                {
                    read = await p.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        await result.WriteAsync(buffer, 0, read);
                    }
                }
            }
            result.Position = 0;
            return result;
        }

        public FileStream GetThumbnail()
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

                    p.StartInfo.Arguments = $"-v quiet -i \"{filePath}\" -an -ss {startS} -vframes 1 -f image2pipe \"{cachePath}\"";
                    p.Start();
                    p.WaitForExit();
                }
            }

            return new FileStream(cachePath, FileMode.Open);
        }
    }
}
