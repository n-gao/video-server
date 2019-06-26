using VideoServer.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

namespace VideoServer.Server.Controllers
{
    [Route("api/")]
    public class QuoteController : Controller
    {
        MongoClient dbClient;

        public QuoteController()
        {
            dbClient = new MongoClient();
        }

        [HttpGet("search")]
        public async Task<IEnumerable<SearchResult>> GetSearchResults(string query)
        {
            var db = dbClient.GetDatabase("sponge_db");
            var quotes = db.GetCollection<BsonDocument>("quotes");
            var episodes = db.GetCollection<BsonDocument>("episodes");

            var filter = Builders<BsonDocument>.Filter.Text(query, "german");
            var projection = Builders<BsonDocument>.Projection.MetaTextScore("score");
            var sort = Builders<BsonDocument>.Sort.MetaTextScore("score");
            var test = await quotes.Find(filter).Project<BsonDocument>(projection).Sort(sort).Limit(10).ToListAsync();

            var result = new SearchResult[test.Count];
            for (int i=0; i< test.Count; i++)
            {
                result[i] = new SearchResult{
                    Episode = test[i]["episode"].AsString,
                    Person = test[i]["person"].AsString,
                    Text = test[i]["text"].AsString,
                    Start = test[i]["timestamp"].AsDouble,
                    EpisodeTitle = "",
                    Score = test[i]["score"].AsDouble
                };
            }
            return result;
        } 

    }
}
