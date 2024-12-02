using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Assistants;

public class ChatService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly AssistantClient _assistantClient;
    private readonly string _assistantId;

    public ChatService(IConfiguration configuration)
    {
        string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        string key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        _assistantId = Environment.GetEnvironmentVariable("AZURE_OPENAI_ASSISTANT_ID");

        _openAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _assistantClient = _openAIClient.GetAssistantClient();
    }

    public async Task<string> GetAssistantResponseAsync(string userQuery)
    {
        try
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
                        return message.Content.FirstOrDefault()?.Text ?? "No response from Assistant.";
                    }
                }

                return "No response from Assistant.";
            }
            else
            {
                return "The request did not complete successfully.";
            }
        }
        catch
        {
            return "An error occurred while processing the request.";
        }
    }
}
