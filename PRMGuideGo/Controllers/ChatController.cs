using GuideGo_Service.Dtos.Chat;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PRMGuideGo.Hubs;

namespace PRMGuideGo.Controllers
{
    /// <summary>
    /// API chat realtime giữa Tourist và Guide qua SignalR.
    /// </summary>
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Lấy hoặc tạo mới cuộc trò chuyện giữa người dùng hiện tại và một Guide.
        /// </summary>
        [HttpPost("get-or-create")]
        public async Task<IActionResult> GetOrCreateChat(Guid guideId)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);

            var result = await _chatService.GetOrCreateChat(userId, guideId);

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tin nhắn trong một cuộc trò chuyện theo chatId.
        /// </summary>
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);

            var result = await _chatService.GetMessages(chatId, userId);

            return Ok(result);
        }

        /// <summary>
        /// Gửi tin nhắn trong một cuộc trò chuyện và broadcast qua SignalR.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderId = Guid.Parse(User.FindFirst("id")!.Value);

            var message = await _chatService.SendMessage(
                request.ChatId,
                senderId,
                request.Content
            );

            await _hubContext.Clients
                .Group(request.ChatId.ToString())
                .SendAsync("ReceiveMessage", message);

            return Ok(message);
        }

        /// <summary>
        /// Lấy danh sách tất cả cuộc trò chuyện của người dùng hiện tại.
        /// </summary>
        [HttpGet("my-chats")]
        public async Task<IActionResult> GetMyChats()
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);

            var result = await _chatService.GetMyChats(userId);

            return Ok(result);
        }
    }
}
