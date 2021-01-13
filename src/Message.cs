using Newtonsoft.Json;

namespace Poker
{
    public class Message
    {
        public string Key { get; }
        public string Type { get; }
        public string Object { get; set; }

        public Message(string key, string type, object obj = null)
        {
            Key = key;
            Type = type;
            Object = JsonConvert.SerializeObject(obj);
        }

        public string Json()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
