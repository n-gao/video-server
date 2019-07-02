using VideoServer.Shared;
using VideoServer.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IVideoSettings config;
        private readonly IVideoService videoService;

        public VideoController(IVideoSettings conf, IVideoService videoService) {
            config = conf;
            this.videoService = videoService;
        }

        [HttpGet("video/{season}/{episode}")]
        public async Task<IActionResult> GetVideo(int season, string episode, float start = 0, float duration = 20)
        {
            return File(await videoService.ReadToStream(GetFilePath(season, episode), start, duration), "video/mp4");
        }
        
        [HttpGet("thumbnail/{season}/{episode}")]
        public async Task<IActionResult> GetThumbnail(int season, string episode, float timestamp=2)
        {
            return File(await videoService.GetThumbnail(GetFilePath(season, episode), timestamp), "image/jpeg");
        }

        private string GetFilePath(int season, string episode) {
            char episodeMinor = episode[episode.Length - 1];
            int episodeMajor;
            if (char.IsLetter(episodeMinor)) {
                episodeMajor = int.Parse(episode.Remove(episode.Length - 1));
                return Path.Join(config.Folder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}{episodeMinor}{config.Format}");
            } else {
                episodeMajor = int.Parse(episode);
                return Path.Join(config.Folder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}{config.Format}");
            }
        }

    }
}
