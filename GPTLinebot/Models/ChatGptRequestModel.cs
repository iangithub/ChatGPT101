using Newtonsoft.Json;

namespace GPTLinebot.Models.Chatgpt
{
    public class ChatGptRequestModel
    {
        [JsonProperty("model")]
        public string Model { get; private set; }
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }
        [JsonProperty(PropertyName = "max_tokens")]
        public int Max_tokens { get; set; }
        [JsonProperty(PropertyName = "temperature")]
        public double Temperature { get; set; }

        public ChatGptRequestModel()
        {
            Messages = new List<Message>();
            Model = "gpt-3.5-turbo";
        }
    }
    public class Completion
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "object")]
        public string AiType { get; set; }
        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }
        [JsonProperty(PropertyName = "choices")]
        public Choice[] Choices { get; set; }
        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }
    }

    public class Usage
    {
        [JsonProperty(PropertyName = "prompt_tokens")]
        public int Prompt_Tokens { get; set; }
        [JsonProperty(PropertyName = "completion_tokens")]
        public int Completion_Tokens { get; set; }
        [JsonProperty(PropertyName = "total_tokens")]
        public int Total_Tokens { get; set; }
    }

    public class Choice
    {
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; }
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }
        [JsonProperty(PropertyName = "finish_reason")]
        public string Finish_reason { get; set; }
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
