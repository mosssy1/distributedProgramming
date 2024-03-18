using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using static System.Net.Mime.MediaTypeNames;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _db;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _redisConnection = redisConnection;
        _db = _redisConnection.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        if (string.IsNullOrEmpty(text))
        {
            return Redirect($"index");
        }

        string rankKey = "RANK-" + id;
        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        double rank = CalculateRank(text);
        _db.StringSet(rankKey, rank);

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        double similarity = CalculateSimilarity(text);
        _db.StringSet(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _db.StringSet(textKey, text);

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        int totalCharacters = text.Length;
        int nonAlphabeticCharacters = 0;

        foreach (char character in text)
        {
            if (!char.IsLetter(character))
            {
                nonAlphabeticCharacters++;
            }
        }

        double contentRank = (double)nonAlphabeticCharacters / totalCharacters;

        return contentRank;
    }

    private double CalculateSimilarity(string text)
    {
        var allKeys = _redisConnection.GetServer("localhost:6379").Keys();
        double similarity = 0.0;
        foreach (var key in allKeys)
        {
            if (key.ToString().Substring(0, 4) != "TEXT")
            {
                continue;
            }
            string dbText = _db.StringGet(key);
            if (dbText == text)
            {
                similarity = 1.0;
            }
        }
        return similarity;
    }
}