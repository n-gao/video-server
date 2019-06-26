using System;
using System.Collections.Generic;
using System.Text;

namespace VideoServer.Shared
{
    public class SearchQuery
    {
        public string Query { get; set; }
        public float Duration { get; set; } = 20;
        public float Skip { get; set; } = -10;
    }
}
