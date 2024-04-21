using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    public class MessageInfo
    {
        public string Id { get; set; }
        public string Result { get; set; }

        public MessageInfo(string id, string result)
        {
            Id = id;
            Result = result;
        }
    }

    private readonly ILogger<IndexModel> _logger;
    private readonly IRedis _redis;

    public IndexModel(ILogger<IndexModel> logger, IRedis redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Redirect("index");
        }
        else
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = "SIMILARITY-" + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
            string similarity = GetSimilarity(text);
            _redis.Put(similarityKey, similarity);

            string textKey = "TEXT-" + id;
            //TODO: сохранить в БД text по ключу textKey
            _redis.Put(textKey, text);

            //TODO: посчитать rank и сохранить в БД по ключу rankKey

            ConnectionFactory connectionFactory = new ConnectionFactory();

            using(IConnection c = connectionFactory.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(id);
                c.Publish("valuator.processing.rank", data);

                MessageInfo? info = new(textKey, similarity);
                string jsonData = JsonSerializer.Serialize(info);

                byte[] jsonDataEncoded = Encoding.UTF8.GetBytes(jsonData);

                c.Publish("similarityCalculated",jsonDataEncoded);

                c.Drain();

                c.Close();
            }

            return Redirect($"summary?id={id}");
        }
    }

    private string GetSimilarity(string text)
    {
        var keys = _redis.GetKeys();
        string textKeyPrefix = "TEXT-";

        foreach (var key in keys)
        {
            if (key.StartsWith(textKeyPrefix) && _redis.Get(key) == text)
            {
                return "1";
            }
        }

        return "0";
    }
}
