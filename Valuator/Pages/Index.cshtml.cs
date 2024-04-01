using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using static System.Net.Mime.MediaTypeNames;
using NATS.Client;
using System.Text;

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

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        double similarity = CalculateSimilarity(text);
        _db.StringSet(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _db.StringSet(textKey, text);

        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        CancellationTokenSource cts = new CancellationTokenSource();

        ConnectionFactory cf = new ConnectionFactory();

        using (IConnection c = cf.CreateConnection())
        {
            byte[] data = Encoding.UTF8.GetBytes(id);
            c.Publish("valuator.processing.rank", data);
            
            c.Drain();

            c.Close();
        }

        cts.Cancel();

        return Redirect($"summary?id={id}");
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