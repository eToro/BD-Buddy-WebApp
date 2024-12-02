using Microsoft.AspNetCore.Mvc;


public class ChatController : Controller
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    // GET: Chat
    public IActionResult Index()
    {
        return View();
    }

    // POST: Chat/AskAssistant
    [HttpPost]
    public async Task<IActionResult> AskAssistant(string userQuery)
    {
        if (!string.IsNullOrEmpty(userQuery))
        {
            var assistantResponse = await _chatService.GetAssistantResponseAsync(userQuery);
            ViewData["AssistantResponse"] = assistantResponse;
            
            return Json(new { response = assistantResponse });
        }
        return Json(new { response = "No response from Assistant." });
    }
}
