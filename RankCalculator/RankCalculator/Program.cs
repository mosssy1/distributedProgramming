using NATS.Client;
using System.Text;
using StackExchange.Redis;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse("localhost:6379");
            ConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect(redisConfiguration);
            IDatabase db = redisConnection.GetDatabase();
            ConnectionFactory cf = new ConnectionFactory();
            IConnection c = cf.CreateConnection();

            var s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = "TEXT-" + id;
                string text = db.StringGet(textKey);

                string rankKey = "RANK-" + id;

                double rank = CalculateRank(text);

                db.StringSet(rankKey, rank);
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        static double CalculateRank(string text)
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
    }
}