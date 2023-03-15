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
        private string channel_Access_Token = "Line platform channel key";

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var replyEvent = new ReplyEvent(channel_Access_Token);

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

                        //交由PT-3.5 Turbo模型進行文字潤飾
                        var chatgptMsg = await AzureOpenAi(ansMsg);

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
            Uri endpoint = new Uri("https://{azure service name}.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("Azure Key Credential");
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);

            string projectName = "project Name";
            string deploymentName = "deployment Name";
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
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

        private async Task<string> AzureOpenAi(string ans)
        {
            string azureOpenApiKey = "azure OpenApi Key";
            string azureOpenApiEndpoint = "https://{azure OpenAI service name}.openai.azure.com/openai/deployments/{deploy name}/completions?api-version=2022-12-01";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", azureOpenApiKey);
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
    }
}
