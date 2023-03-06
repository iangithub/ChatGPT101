using DotNetLineBotSdk.Helpers;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageEvent;
using GPTLinebot.Models;
using GPTLinebot.Models.Chatgpt;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace GPTLinebot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Linebot3Controller : ControllerBase
    {
        private string channel_Access_Token = "line channel Access Token";
        string apiKey = "openai api key";
        string endpoint = "https://api.openai.com/v1/chat/completions";

        string userPrompt = string.Empty;
        string historyPrompt = string.Empty;
        private readonly ChatGptRequestModel _chatGptRequestModel;

        public Linebot3Controller(ChatGptRequestModel chatGptRequestModel)
        {
            _chatGptRequestModel = chatGptRequestModel;
            _chatGptRequestModel.Temperature = 0.5;
            _chatGptRequestModel.Max_tokens = 500;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var replyEvent = new ReplyEvent(channel_Access_Token);

            try
            {
                //Get Post RawData (json format)
                var req = this.HttpContext.Request;
                using (var bodyReader = new StreamReader(stream: req.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024, leaveOpen: true))
                {
                    var body = await bodyReader.ReadToEndAsync();
                    var lineReceMsg = ReceivedMessageConvert.ReceivedMessage(body);

                    if (lineReceMsg != null && lineReceMsg.Events[0].Type == WebhookEventType.message.ToString())
                    {
                        //get user msg
                        var userMsg = lineReceMsg.Events[0].Message.Text;
                        _chatGptRequestModel.Messages.Add(new Message() { Role = "user", Content = userMsg });

                        //send to openai api
                        GPTLinebot.Models.Chatgpt.Completion completionMsg = await GenerateText(_chatGptRequestModel);
                        _chatGptRequestModel.Messages.Add(completionMsg.Choices[0].Message);


                        //send reply msg
                        var txtMessage = new TextMessage(completionMsg.Choices[0].Message.Content);
                        await replyEvent.ReplyAsync(lineReceMsg.Events[0].ReplyToken,
                                                   new List<IMessage>() {
                                                       txtMessage
                                                   });
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok();
            }
            return Ok();
        }

        private async Task<GPTLinebot.Models.Chatgpt.Completion> GenerateText(ChatGptRequestModel chatGptRequestModel)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var json = JsonConvert.SerializeObject(chatGptRequestModel);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, data);
                var responseContent = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GPTLinebot.Models.Chatgpt.Completion>(responseContent);
            }
        }
    }
}
