using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redisConnection;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _redisConnection = redisConnection;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        //TODO: проинициализировать свойства Rank и Similarity значениями из БД
        var db = _redisConnection.GetDatabase();
        Rank = double.Parse(db.StringGet($"RANK-{id}"), System.Globalization.CultureInfo.InvariantCulture);
        Similarity = double.Parse(db.StringGet($"SIMILARITY-{id}"), System.Globalization.CultureInfo.InvariantCulture);

    }
}