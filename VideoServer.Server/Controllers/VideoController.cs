using VideoServer.Shared;
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
        private readonly string BaseFolder;
        private readonly IVideoStream stream;

        public VideoController(IConfiguration conf, IVideoStream stream) {
            BaseFolder = conf[SettingKeys.BASE_FOLDER];
            this.stream = stream;
        }

        [HttpGet("video/{season}/{episode}")]
        public async Task<IActionResult> GetVideo(int season, string episode, float start = 0, float duration = 20)
        {
            stream.FilePath = GetFilePath(season, episode);
            stream.Start = start;
            stream.Duration = duration;
            return File(await stream.ReadToStream(), "video/mp4");
        }
        
        [HttpGet("thumbnail/{season}/{episode}")]
        public async Task<IActionResult> GetThumbnail(int season, string episode, float timestamp=2)
        {
            stream.FilePath = GetFilePath(season, episode);
            stream.Start = timestamp;
            return File(await stream.GetThumbnail(), "image/jpeg");
        }

        private string GetFilePath(int season, string episode) {
            char episodeMinor = episode[episode.Length - 1];
            int episodeMajor;
            if (char.IsLetter(episodeMinor)) {
                episodeMajor = int.Parse(episode.Remove(episode.Length - 1));
                return Path.Join(BaseFolder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}{episodeMinor}.mkv");
            } else {
                episodeMajor = int.Parse(episode);
                return Path.Join(BaseFolder, $"s{season:0#}\\s{season:0#}e{episodeMajor:0#}.mkv");
            }
        }

    }
}
