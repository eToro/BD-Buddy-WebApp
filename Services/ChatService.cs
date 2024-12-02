
using System.Text.Json;

public class ChatService
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public ChatService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AzureOpenAI:ApiKey"];
        _endpoint = configuration["AzureOpenAI:Endpoint"];
    }

    public async Task<string> SendMessageAsync(string prompt)
    {
        var request = new
        {
            prompt,
            max_tokens = 100,
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{{_endpoint}}/openai/deployments/gpt-4/completions?api-version=2023-03-15-preview",
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
        return responseData.GetProperty("choices")[0].GetProperty("text").GetString();
    }
}
