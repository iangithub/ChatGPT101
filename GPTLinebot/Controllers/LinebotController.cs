using DotNetLineBotSdk.Helpers;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageEvent;
using GPTLinebot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace GPTLinebot.Controllers
{
    /// <summary>
    /// 不具上下文管理的bot
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LinebotController : ControllerBase
    {
        private string channel_Access_Token = "line channel Access Token";
        string model = "text-davinci-003";
        int maxTokens = 500; //與字數有關(問+答合計數)，一個token不等同於一個字，不同模型有不同的上限，而 text-davinci-003 最大不能超過 4,000 
        double temperature = 0.5;　//介於 0~1，越接近0，模型回覆的內容變化越小，背答案的機器人
        string apiKey = "openai api key";
        string endpoint = "https://api.openai.com/v1/completions";

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
                        var userMsg = $"user:{lineReceMsg.Events[0].Message.Text} \n ai:";

                        var promptModel = new OpenAiRequestModel() { Prompt = userMsg, Model = model, Max_tokens = maxTokens, Temperature = temperature };
                        //send to openai api
                        var completionMsg = await GenerateText(promptModel);

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
