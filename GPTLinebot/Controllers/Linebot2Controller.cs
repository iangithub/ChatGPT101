using DotNetLineBotSdk.Helpers;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageEvent;
using GPTLinebot.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace GPTLinebot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Linebot2Controller : ControllerBase
    {
        private string channel_Access_Token = "line channel Access Token";
        string model = "text-davinci-003";
        int maxTokens = 500;
        double temperature = 0.5;
        string apiKey = "openai api key";
        string endpoint = "https://api.openai.com/v1/completions";

        string userPrompt = string.Empty;
        string historyPrompt = string.Empty;
        private readonly UserHistoryPrompt _userHistoryPrompt;

        public Linebot2Controller(UserHistoryPrompt userHistoryPrompt)
        {
            _userHistoryPrompt = userHistoryPrompt;
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

                        //History Prompt
                        foreach (var item in _userHistoryPrompt.HistoryPrompt)
                        {
                            historyPrompt += " " + item;
                        }

                        //combine Prompt
                        userPrompt = historyPrompt + " ME: " + userMsg+"/n AI:" ;

                        var promptModel = new OpenAiRequestModel() { Prompt = userPrompt, Model = model, Max_tokens = maxTokens, Temperature = temperature };
                       
                        //send to openai api
                        var completionMsg = await GenerateText(promptModel);

                        //keep question & ans
                        _userHistoryPrompt.HistoryPrompt.Add(userMsg + completionMsg.Choices[0].Text);

                        //send reply msg
                        var txtMessage = new TextMessage(completionMsg.Choices[0].Text);
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

        private async Task<Completion> GenerateText(OpenAiRequestModel model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var json = JsonConvert.SerializeObject(model);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, data);
                var responseContent = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Completion>(responseContent);
            }
        }
    }
}
