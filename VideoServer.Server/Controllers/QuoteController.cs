using VideoServer.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using VideoServer.Shared.Models;
using VideoServer.Server.Services;

namespace VideoServer.Server.Controllers
{
    [Route("api/")]
    public class QuoteController : Controller
    {
        private DbService _quotes;

        public QuoteController(DbService quotes)
        {
            _quotes = quotes;
        }

        [HttpGet("search")]
        public async Task<IEnumerable<QuoteResult>> GetSearchResults(string query, int numResults=10)
        {
            numResults = numResults > 20 ? 20 : numResults;
            numResults = numResults < 1 ? 1 : numResults;
            return await _quotes.SearchQuotes(query, numResults);
        } 

    }
}
