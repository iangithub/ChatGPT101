
using DotNetLineBotSdk.Message;
using Newtonsoft.Json;

public class OpenAiRequestModel
{
    [JsonProperty(PropertyName = "prompt")]
    public string Prompt { get; private set; }
    [JsonProperty(PropertyName = "temperature")]
    public float Temperature { get; set; }
    [JsonProperty(PropertyName = "top_p")]
    public float Top_p { get; set; }
    [JsonProperty(PropertyName = "frequency_penalty")]
    public int Frequency_Penalty { get; set; }
    [JsonProperty(PropertyName = "presence_penalty")]
    public int Presence_Penalty { get; set; }
    [JsonProperty(PropertyName = "max_tokens")]
    public int Max_Tokens { get; set; }
    [JsonProperty(PropertyName = "stop")]
    public List<string> Stop { get; set; }

    public OpenAiRequestModel()
    {
        Prompt = "<|im_start|>system\n你是一位客服人員，我會給你準備要回答客戶的答案，請你進行內容文字的調整並以客服語氣產生500個字以內的回答\n<|im_end|>\n";
        Temperature = 0.8f;
        Top_p = 0.95f;
        Frequency_Penalty = 0;
        Presence_Penalty = 0;
        Max_Tokens = 1000;
        Stop = new List<string>() { "<|im_end|>" };
    }

    public void AddPrompt(string prompt)
    {
        this.Prompt +=$"<|im_start|>user\n{prompt}\n<|im_end|>\n<|im_start|>assistant" ;
    }
}


public class OpenAiRequestModel_GPT4
{
    [JsonProperty(PropertyName = "messages")]
    public List<Message_GPT4> Messages { get; private set; }
    [JsonProperty(PropertyName = "temperature")]
    public float Temperature { get; set; }
    [JsonProperty(PropertyName = "top_p")]
    public float Top_p { get; set; }
    [JsonProperty(PropertyName = "frequency_penalty")]
    public int Frequency_Penalty { get; set; }
    [JsonProperty(PropertyName = "presence_penalty")]
    public int Presence_Penalty { get; set; }
    [JsonProperty(PropertyName = "max_tokens")]
    public int Max_Tokens { get; set; }
    [JsonProperty(PropertyName = "stop")]
    public List<string> Stop { get; set; }

    public OpenAiRequestModel_GPT4()
    {
        Messages = new List<Message_GPT4>();
        Messages.Add(new Message_GPT4() { Role= "system", Content= "你是一位客服人員，我會給你準備要回答客戶的答案，請你進行內容文字的調整並以客服語氣產生500個字以內的回答" });
        Temperature = 0.8f;
        Top_p = 0.95f;
        Frequency_Penalty = 0;
        Presence_Penalty = 0;
        Max_Tokens = 1000;
        Stop = new List<string>() { "<|im_end|>" };
    }

    public void AddUserMessages(string message)
    {
        this.Messages.Add(new Message_GPT4() { Role = "user", Content = message });
    }
    public void AddGptMessages(string message)
    {
        this.Messages.Add(new Message_GPT4() { Role = "assistant", Content = message });
    }
}

public class Message_GPT4
{
    [JsonProperty(PropertyName = "role")]
    public string Role { get; set; }
    [JsonProperty(PropertyName = "content")]
    public string Content { get; set; }
}
