using DotNetLineBotSdk.Helpers;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageAction;
using DotNetLineBotSdk.MessageEvent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Azure.Core;
using Azure.AI.Language.QuestionAnswering;
using Azure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
using System.Net;

namespace QuestionAnsweringLinebot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinebotController : ControllerBase
    {
        private string line_Channel_Access_Token = "XXXX";
        
        private string az_QuestionAnsering_Endpoint = "https://XXXXX.cognitiveservices.azure.com";
        private string az_QuestionAnsering_Credential = "XXXXXX";
        private string az_QuestionAnsering_ProjectName = "XXXXX";
        private string az_QuestionAnsering_DeploymentName = "XXXXX";
        
        private string az_OpenAi_Endpoint = "https://XXXX.openai.azure.com/openai/deployments";
        private string az_OpenAi_DeploymentName = "XXX";
        private string az_OpenAi_Key = "XXXX";
        private string az_OpenAi_Api_Version = "XXXXX";
        private string az_OpenAi_DeploymentName_Gpt4 = "XXXXXX";

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var replyEvent = new ReplyEvent(line_Channel_Access_Token);

            try
            {

                // Get the http request body
                var body = string.Empty;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                    var lineReceMsg = ReceivedMessageConvert.ReceivedMessage(body);

                    if (lineReceMsg != null && lineReceMsg.Events[0].Type == WebhookEventType.message.ToString())
                    {
                        //客戶問題
                        var userMsg = lineReceMsg.Events[0].Message.Text;

                        //找出客戶問題的對應解答
                        var ansMsg = await AzureQuestionAnsweringServiceAsync(userMsg);

                        ////交由GPT模型進行文字潤飾
                        var chatgptMsg = await AzureOpenAi_GPT3_5(ansMsg);

                        //GPT4
                        //var chatgptMsg = await AzureOpenAi_GPT4(ansMsg);

                        //自動回覆
                        var txtMessage = new TextMessage(chatgptMsg);
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

        private async Task<string> AzureQuestionAnsweringServiceAsync(string msg)
        {
            Uri endpoint = new Uri(az_QuestionAnsering_Endpoint);
            AzureKeyCredential credential = new AzureKeyCredential(az_QuestionAnsering_Credential);
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);

            QuestionAnsweringProject project = new QuestionAnsweringProject(az_QuestionAnsering_ProjectName, az_QuestionAnsering_DeploymentName);
            Response<AnswersResult> response = await client.GetAnswersAsync(msg, project);

            foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
            {
                Console.WriteLine($"({answer.Confidence:P2}) {answer.Answer}");
                Console.WriteLine($"Source: {answer.Source}");
                Console.WriteLine();
            }

            var x = response.Value.Answers[0];

            return response.Value.Answers[0] != null ? response.Value.Answers[0].Answer : "很抱歉,無法回答您的問題";

        }

        private async Task<string> AzureOpenAi_GPT3_5(string ans)
        {
            string azureOpenApiEndpoint = $"{az_OpenAi_Endpoint}/{az_OpenAi_DeploymentName}/completions?api-version={az_OpenAi_Api_Version}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", az_OpenAi_Key);
                var requestModel = new OpenAiRequestModel();
                requestModel.AddPrompt(ans);

                var json = JsonConvert.SerializeObject(requestModel);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(azureOpenApiEndpoint, data);
                var responseContent = await response.Content.ReadAsStringAsync();

                var completion = JsonConvert.DeserializeObject<Completion>(responseContent);

                return completion.Choices[0].Text;
            }
        }

        private async Task<string> AzureOpenAi_GPT4(string ans)
        {
            string azureOpenApiEndpoint = $"{az_OpenAi_Endpoint}/{az_OpenAi_DeploymentName_Gpt4}/chat/completions?api-version={az_OpenAi_Api_Version}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", az_OpenAi_Key);
                var requestModel = new OpenAiRequestModel_GPT4();
                requestModel.AddUserMessages(ans);

                var json = JsonConvert.SerializeObject(requestModel);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(azureOpenApiEndpoint, data);
                var responseContent = await response.Content.ReadAsStringAsync();

                var completion = JsonConvert.DeserializeObject<Completion_GPT4>(responseContent);

                return completion.Choices[0].Message.Content;
            }
        }
    }
}
