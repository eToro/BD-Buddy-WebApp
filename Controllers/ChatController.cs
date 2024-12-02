using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var response = await _chatService.SendMessageAsync(request.Message);
        return Ok(new { response });
    }
}

public record ChatRequest(string Message);
