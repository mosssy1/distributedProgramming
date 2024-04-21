namespace RankCalculator
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
}
