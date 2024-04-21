namespace Valuator
{
    public interface IRedis
    {
        string Get(string key);
        void Put(string key, string value);
        List<string> GetKeys();
    }
}