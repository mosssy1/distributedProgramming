using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IRedis _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IRedis redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);
        Rank = Convert.ToDouble(_redis.Get($"RANK-{id}"));
        Similarity = Convert.ToDouble(_redis.Get($"SIMILARITY-{id}"));

        //TODO: проинициализировать свойства Rank и Similarity значениями из БД
    }
}
