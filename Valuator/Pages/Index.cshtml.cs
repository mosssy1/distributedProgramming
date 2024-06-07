using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using static System.Net.Mime.MediaTypeNames;
using NATS.Client;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _db;
    
    class TextData
    {
        public TextData(string id, string data)
        {
            this.id = id;
            this.data = data;
        }
        public string id { get; set; }
        public string data { get; set; }
    }

    class IdAndCountryOfText
    {
        public IdAndCountryOfText(string country, string textId)
        {
           this.textId = textId;
           this.country = country; 
        }
        public string country { get; set; } 
        public string textId { get; set; } 
    }

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _redisConnection = redisConnection;
        _db = _redisConnection.GetDatabase();   
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text, string country)
    {
        _logger.LogDebug(text);
        _logger.LogDebug(country);

        string id = Guid.NewGuid().ToString();

        if (string.IsNullOrEmpty(text))
        {
            return Redirect($"index");
        }

        string dbEnvironmentVariable = $"DB_{country}";

        _db.StringSet(id, country);

        string? dbConnection = Environment.GetEnvironmentVariable(dbEnvironmentVariable);

        if (dbConnection != null) 
        {
            ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(dbConnection);
            redisConfiguration.AbortOnConnectFail = false; // Разрешить повторные попытки подключения
            IDatabase savingDb = ConnectionMultiplexer.Connect(redisConfiguration).GetDatabase();

            string similarityKey = "SIMILARITY-" + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
            var similarity = CalculateSimilarity(text);
            savingDb?.StringSet(similarityKey, similarity);
            Console.WriteLine($"LOOKUP: {id}, {country}");

            string textKey = "TEXT-" + id;
            //TODO: сохранить в БД text по ключу textKey
            savingDb?.StringSet(textKey, text);
            Console.WriteLine($"LOOKUP: {id}, {country}");

            //TODO: посчитать rank и сохранить в БД по ключу rankKey
            CancellationTokenSource cts = new CancellationTokenSource();

            ConnectionFactory cf = new ConnectionFactory();

            using (IConnection c = cf.CreateConnection())
            {
                IdAndCountryOfText structData = new IdAndCountryOfText(country, id);

                string infoJson = JsonSerializer.Serialize(structData);

                byte[] data = Encoding.UTF8.GetBytes(infoJson);

                c.Publish("valuator.processing.rank", data);

                TextData textData = new TextData(id, similarity);

                infoJson = JsonSerializer.Serialize(textData);

                data = Encoding.UTF8.GetBytes(infoJson);

                c.Publish("valuator.logs.events.similarity", data);

                c.Drain();

                c.Close();
            }

            cts.Cancel();

            return Redirect($"summary?id={id}&country={country}");
        }
        return Redirect($"index");
    }

    private static string CalculateSimilarity(string text)
    {
        var a = Environment.GetEnvironmentVariables();
        string similarity = "";

        foreach (var key in a.Keys ) 
        {
            if (key.ToString().StartsWith("DB_"))
            {
                ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(a[key].ToString());
                IConnectionMultiplexer redisDB = ConnectionMultiplexer.Connect(redisConfiguration);

                similarity = redisDB.GetServer(a[key].ToString()).Keys().Select(x => x.ToString())
                    .ToList().Find(key => key.StartsWith("TEXT-") && redisDB.GetDatabase().StringGet(key) == text) != null ? "1" : "0";
                if (similarity == "1")
                    break;
            }
        }
        return similarity;
    }
   
}
