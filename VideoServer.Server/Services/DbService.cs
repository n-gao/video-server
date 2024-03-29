using VideoServer.Shared.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace VideoServer.Server.Services {
    public class DbService {
        private readonly IMongoCollection<Quote> _quotes;
        private readonly IMongoCollection<Episode> _episodes;

        public DbService(IDatabaseSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _quotes = database.GetCollection<Quote>(settings.QuoteCollectionName);
            _episodes = database.GetCollection<Episode>(settings.EpisodeCollectionName);
        }

        public async Task<IEnumerable<QuoteResult>> SearchQuotes(string query, int numResults) {
            var filter = Builders<Quote>.Filter.Text(query, "german");
            var projection = Builders<Quote>.Projection.MetaTextScore("MatchingScore");
            var sort = Builders<Quote>.Sort.MetaTextScore("MatchingScore");

            var result = await _quotes.Find(filter)
                .Project<QuoteResult>(projection)
                .Sort(sort)
                .Limit(numResults)
                .ToListAsync();

            var episodeIdx = result.Select(q => q.EpisoodeId);
            var episodes = (await _episodes.Find(e => episodeIdx.Contains(e.Id)).ToListAsync())
                .ToDictionary(x => x.Id, x => x);

            result.ForEach(r => r.Episode = episodes[r.EpisoodeId]);

            return result;
        }
    }
}