using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ChatController : Controller
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    public IActionResult Index()
    {
        return View();
    }

    //[HttpPost("SendMessage")]
    //public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    //{
    //    var response = await _chatService.SendMessageAsync(request.Message);
    //    return Ok(new { response });
    //}       [HttpPost]
}

public record ChatRequest(string Message);
