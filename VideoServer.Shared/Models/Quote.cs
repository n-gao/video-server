﻿using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VideoServer.Shared.Models
{
    [BsonIgnoreExtraElements]
    public class Quote
    {
        public Quote() {}

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("episode")]
        public string EpisoodeId { get; set; }

        public Episode Episode {get; set;}

        [BsonElement("person")]
        public string Person { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

        [BsonElement("timestamp")]
        public double TimeStamp { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class QuoteResult : Quote
    {
        [BsonIgnoreIfNull]
        public double? MatchingScore {get; set;}

        public bool Stream {get; set;} = false;

        public int Season => int.Parse(EpisoodeId.Substring(1, 2));
        public string EpisodeNumber => EpisoodeId.Substring(4);
    }
}
