using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Valuator
{
    public class RedisService : IRedis
    {
        private readonly IConnectionMultiplexer _connection;
        public RedisService()
        {
            _connection = ConnectionMultiplexer.Connect(Configurations.HOST_NAME);
        }

        public string Get(string key)
        {
            var db = _connection.GetDatabase();

            return db.StringGet(key);
        }
        public List<string> GetKeys()
        {
            var keys = _connection.GetServer(Configurations.HOST_NAME, Configurations.HOST_PORT).Keys();

            return keys.Select(x => x.ToString()).ToList();
        }

        public void Put(string key, string value)
        {
            var db = _connection.GetDatabase();

            db.StringSet(key, value);
        }
    }
}