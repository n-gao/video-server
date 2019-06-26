using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace VideoServer.Shared
{
    public class SearchResult
    {
        public string Text;
        public string Person;
        public string Episode;
        public string EpisodeTitle;
        public double Start;
        public double Score;

        public bool Stream = false;

        public int Season => int.Parse(Episode.Substring(1, 2));
        public string EpisodeNumber => Episode.Substring(4);
    }
}
