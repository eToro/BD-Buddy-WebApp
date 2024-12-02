using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


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
            var counter = 0;
            var isSuccess = false;
            while (counter <= 1 && !isSuccess)
            {
                try
                {
                    var assistantResponse = await _chatService.GetAssistantResponseAsync(userQuery);
                    ViewData["AssistantResponse"] = assistantResponse;

                    isSuccess = true;
                    return Json(new { response = assistantResponse });
                }
                catch (Exception e)
                {
                    userQuery += ", Check the docs for the correct Column names and Table names and try again verify case sensitive names";
                    counter++;
                }
            }
        }
        return Json(new { response = "No response from Assistant." });
    }
}
