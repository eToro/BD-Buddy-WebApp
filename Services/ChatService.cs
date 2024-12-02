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
        var threadResponse = await _assistantClient.CreateThreadAsync();
        var userMessageContent = MessageContent.FromText(userQuery);

        var messageResponse = await _assistantClient.CreateMessageAsync(
            threadResponse.Value.Id,
            MessageRole.User,
            new List<MessageContent>() { userMessageContent });

        var runResponse = await _assistantClient.CreateRunAsync(threadResponse.Value.Id, _assistantId);

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await _assistantClient.GetRunAsync(threadResponse.Value.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

        if (runResponse.Value.Status == RunStatus.Completed)
        {
            AsyncCollectionResult<ThreadMessage> messages = _assistantClient.GetMessagesAsync(
                threadResponse.Value.Id,
                new MessageCollectionOptions() { Order = MessageCollectionOrder.Descending });

            await foreach (ThreadMessage message in messages)
            {
                if (message.Role == MessageRole.Assistant)
                {
                    var messageContent = message.Content[0];
                    if (!string.IsNullOrEmpty(messageContent.Text))
                    {
                        return messageContent.Text;
                    }
                }
            }
        }
        return "The request did not complete successfully.";
    }
}
