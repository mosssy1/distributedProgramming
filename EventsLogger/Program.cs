using NATS.Client;
using System.Text;
using System.Text.Json;

namespace EventsLogger
{
    public class Program
    {
        static void Main()
        {
            Console.WriteLine("EventsLogger started");

            ConnectionFactory connectionFactory = new();
            IConnection c = connectionFactory.CreateConnection();

            var rankSubscriber = c.SubscribeAsync("rankCalculated", "event_logger", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);
                MessageInfo? info = JsonSerializer.Deserialize<MessageInfo>(id);
                Console.WriteLine(
                    $"1.Rank\n" +
                    $"2.Id - {info.Id}\n" +
                    $"3.Result - {info.Result}");
            });

            rankSubscriber.Start();

            var similaritySubscriber = c.SubscribeAsync("similarityCalculated", "event_logger", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);
                MessageInfo? info = JsonSerializer.Deserialize<MessageInfo>(id);
                Console.WriteLine(
                    $"1.Similarity\n" +
                    $"2.Id - {info.Id}\n" +
                    $"3.Result - {info.Result}");
            });

            similaritySubscriber.Start();

            Console.WriteLine("Press Enter to exit(EventsLogger)");
            Console.ReadLine();
        }
    }
}