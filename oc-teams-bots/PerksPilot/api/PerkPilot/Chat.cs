using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PerkPilot.Services;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes;
using static System.Collections.Specialized.BitVector32;
using PerkPilot.Helpers;
using Microsoft.WindowsAzure.Storage.Table;
using static Azure.Core.HttpHeader;
using Azure.AI.OpenAI;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using System.Net;
using PerkPilot.Constants;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using PerkPilot.Models;

namespace PerkPilot
{
    public static class Chat
    {
        [FunctionName("Chat")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                
                string invocationId = context.InvocationId.ToString();
                
                //User request
                string prompt = req.Query["prompt"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                prompt = prompt ?? data?.prompt;
                string userId = data?.userId;
                string userName = data?.userName;
                bool reset = data?.reset;
                
                                
                //Search document using Azure Cognitive search
                string SearchServiceEndPoint = Environment.GetEnvironmentVariable("SearchServiceEndPoint");
                string SearchIndexName = Environment.GetEnvironmentVariable("SearchIndexName");
                string SearchServiceQueryApiKey = Environment.GetEnvironmentVariable("SearchServiceQueryApiKey");
                SearchClient searchClient = new SearchClient(new Uri(SearchServiceEndPoint), SearchIndexName, new AzureKeyCredential(SearchServiceQueryApiKey));
                List<SearchResult> cogSearchResponse = await new AzureSearchService().QueryDocumentsAsync(searchClient, prompt);
                var cogResult = cogSearchResponse.Count > 0 ? cogSearchResponse.FirstOrDefault() : new SearchResult();

                //Chat History
                string ChatHistoryTableName = Environment.GetEnvironmentVariable("ChatHistoryTableName");                
                CloudTable table = await AzureTableHelper.CreateTableAsync(ChatHistoryTableName);
                string PreviousMessagesKey = "PreviousMessage";
                if (reset)
                {
                    var msgEntity = new MessageEntity(userId, PreviousMessagesKey)
                    {
                        ETag = "*"
                    };
                    await AzureTableHelper.DeleteEntityAsync(table, msgEntity);
                    return new OkObjectResult("your chat has been reset");
                }
                var chatHistory = await ChatHelper.GetPreviousMessages(userId, PreviousMessagesKey, table);

                //Azure Open AI configurations
                var messages = new List<ChatMessage>
            {
                //System
                new ChatMessage(ChatRole.System, AppConstants.ThoughtProcessPrompt),

                //history
                new ChatMessage(ChatRole.Assistant, string.IsNullOrEmpty(chatHistory.ChatResponse) ? string.Empty : chatHistory.ChatResponse ),                

                //Azure Cognitive search results
                new ChatMessage(ChatRole.Assistant, string.IsNullOrEmpty(cogResult.Content) ? "" : cogResult.Content),

                //user
                new ChatMessage(ChatRole.User, prompt)
            };
                
                var chatCompletionOptions = new ChatCompletionsOptions
                {
                    User = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId))),
                    MaxTokens = 800,
                    Temperature = 0f,
                    FrequencyPenalty = 0.2f,
                    PresencePenalty = 0.0f,
                    NucleusSamplingFactor = 1 // Top P
                };
                foreach (var message in messages)
                {
                    chatCompletionOptions.Messages.Add(message);
                }                                
                string AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("Azure:OpenAI:Endpoint");
                string AzureOpenAIKey = Environment.GetEnvironmentVariable("Azure:OpenAI:ApiKey");
                string AzureOpenAIModel = Environment.GetEnvironmentVariable("Azure:OpenAI:Model");
                var endpoint = new Uri(AzureOpenAIEndpoint);
                var credentials = new Azure.AzureKeyCredential(AzureOpenAIKey);
                var openAIClient = new OpenAIClient(endpoint, credentials);
                
                var chatCompletionsResponse = await openAIClient.GetChatCompletionsAsync(
                AzureOpenAIModel,
                chatCompletionOptions);
                var azureOpenAIResponse = chatCompletionsResponse.Value.Choices.Count > 0 ? chatCompletionsResponse.Value.Choices.FirstOrDefault().Message.Content.ToString() : "no data";

                var messageEntity = new MessageEntity(userId, PreviousMessagesKey)
                {
                    ChatResponse = azureOpenAIResponse,
                    ChatQuestion  = prompt
                };
                var insertedMessage = await AzureTableHelper.InsertOrMergeEntityAsync(table, messageEntity);

                //response
                cogResult.OpenAISummary = azureOpenAIResponse;
                return new OkObjectResult(cogResult);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Something went wrong");
                var model = new { error = "User friendly something went wrong" };
                return new ObjectResult(model)
                {
                    StatusCode = 500
                };
            }
        }
    }
}
