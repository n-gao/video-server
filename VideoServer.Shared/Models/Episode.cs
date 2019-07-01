using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VideoServer.Shared.Models
{
    public class Episode
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("season")]
        public int Season { get; set; }

        [BsonElement("episode")]
        public string EpisodeNumber { get; set; }
    }
}
