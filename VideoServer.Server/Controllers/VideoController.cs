using VideoServer.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VideoServer.Server.Controllers
{
    [Route("api/")]
    public class VideoController : Controller
    {
        private const string RootFolder = @"D:\Movies\SpongebobMkv";

        [HttpGet("video/{season}/{episode}")]
        public async Task<IActionResult> GetVideo(int season, string episode, float start = 0, float duration = 20)
        {
            int episodeMajor = int.Parse(episode.Remove(episode.Length - 1));
            char episodeMinor = episode[episode.Length - 1];
            string episodePath = Path.Join(RootFolder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}{episodeMinor}.mkv");

            var result = await new VideoStream(episodePath, start - 10, duration).ReadToStream();

            return File(result, "video/x-msvideo");
        }
        
        [HttpGet("thumbnail/{season}/{episode}")]
        public IActionResult GetThumbnail(int season, string episode, float timestamp=2)
        {
            int episodeMajor = int.Parse(episode.Remove(episode.Length - 1));
            char episodeMinor = episode[episode.Length - 1];
            string episodePath = Path.Join(RootFolder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}{episodeMinor}.mkv");

            var result = new VideoStream(episodePath, timestamp, 0).GetThumbnail();

            return File(result, "image/jpeg");
        }
    }
}
