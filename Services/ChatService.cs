using Azure;
using Azure.AI.OpenAI;
using ChatGPTClone.Infrastructure;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using OpenAI.Assistants;

public class ChatService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly AssistantClient _assistantClient;
    private readonly string _assistantId;
    private readonly DatabaseRepository _databaseRepository;

    public ChatService(IConfiguration configuration, DatabaseRepository databaseRepository)
    {
        _databaseRepository = databaseRepository;
        string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        string key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        _assistantId = Environment.GetEnvironmentVariable("AZURE_OPENAI_ASSISTANT_ID");

        _openAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _assistantClient = _openAIClient.GetAssistantClient();
    }

    public async Task<object> GetAssistantResponseAsync(string userQuery)
    {

        var threadResponse = await _assistantClient.CreateThreadAsync();
        var userMessageContent = MessageContent.FromText(userQuery);

        await _assistantClient.CreateMessageAsync(
            threadResponse.Value.Id,
            MessageRole.User,
            new List<MessageContent> { userMessageContent });

        var runResponse = await _assistantClient.CreateRunAsync(threadResponse.Value.Id, _assistantId);

        do
        {
            await Task.Delay(500);
            runResponse = await _assistantClient.GetRunAsync(threadResponse.Value.Id, runResponse.Value.Id);
        } while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

        if (runResponse.Value.Status == RunStatus.Completed)
        {
            var messageResponse = _assistantClient.GetMessagesAsync(
                threadResponse.Value.Id,
                new MessageCollectionOptions { Order = MessageCollectionOrder.Descending });

            await foreach (var message in messageResponse)
            {
                if (message.Role == MessageRole.Assistant)
                {
                    var assistantMessage = message.Content.FirstOrDefault();
                    if (assistantMessage != null)
                    {
                        if (!string.IsNullOrEmpty(assistantMessage.Text))
                        {
                            return assistantMessage.Text;
                        }
                    }
                }
            }

            return "No assistant message found.";
        }
        else if (runResponse.Value.Status == RunStatus.RequiresAction)
        {
            var requiredActions = runResponse.Value.RequiredActions;
            var arguments = requiredActions[0].FunctionArguments;
            var jobj = JObject.Parse(arguments.ToString());

            var sqlQuery = jobj["sqlQuery"].ToString();
            var res = _databaseRepository.ExecuteCommand(sqlQuery);
            res.ToString();
            return FormatDynamicObjects(res);

        }
        else
        {
            return "The request did not complete successfully.";
        }
    }

    static string FormatDynamicObjects(List<dynamic> dynamicObjects)
    {
        if (dynamicObjects == null || dynamicObjects.Count == 0)
        {
            return "No results found.";
        }

        var result = new System.Text.StringBuilder();

        foreach (var obj in dynamicObjects)
        {
            foreach (var property in (IDictionary<string, object>)obj)
            {
                result.AppendLine($"{property.Key}: {property.Value}");
            }
            result.AppendLine(new string('-', 20)); // Separator between objects  
        }

        return result.ToString();
    }
}
