using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Chat;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IGuideRepository _guideRepo;

        public ChatService(IChatRepository chatRepo, IGuideRepository guideRepo)
        {
            _chatRepo = chatRepo;
            _guideRepo = guideRepo;
        }

        private async Task<Chat> ValidateUserInChat(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);

            if (chat == null)
                throw new Exception("Chat không tồn tại");

            if (chat.UserId == userId)
                return chat;

            var guide = await _guideRepo.GetByUserIdAsync(userId);

            if (guide != null && guide.Id == chat.GuideId)
                return chat;

            throw new Exception("Bạn không có quyền truy cập chat này");
        }

        public async Task<ChatDto> GetOrCreateChat(Guid userId, Guid guideId)
        {
            var chat = await _chatRepo.GetChatAsync(userId, guideId);

            if (chat == null)
            {
                chat = new Chat
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    GuideId = guideId
                };

                chat = await _chatRepo.CreateChatAsync(chat);
            }

            return new ChatDto
            {
                Id = chat.Id,
                UserId = chat.UserId,
                GuideId = chat.GuideId
            };
        }

        public async Task<List<MessageDto>> GetMessages(Guid chatId, Guid userId)
        {
            await ValidateUserInChat(chatId, userId);

            var messages = await _chatRepo.GetMessagesAsync(chatId);

            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ChatId = m.ChatId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToList();
        }

        public async Task<MessageDto> SendMessage(Guid chatId, Guid senderId, string content)
        {
            await ValidateUserInChat(chatId, senderId); 

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await _chatRepo.AddMessageAsync(message);

            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }

        public async Task<List<ChatDto>> GetMyChats(Guid userId)
        {
            var guide = await _guideRepo.GetByUserIdAsync(userId);

            var chats = await _chatRepo.GetMyChats(userId, guide?.Id);

            return chats.Select(c => new ChatDto
            {
                Id = c.Id,
                UserId = c.UserId,
                GuideId = c.GuideId,

                UserName = c.User.FullName,
                GuideName = c.Guide.User.FullName,

                LastMessage = c.Messages
        .OrderByDescending(m => m.SentAt)
        .Select(m => m.Content)
        .FirstOrDefault(),

                LastTime = c.Messages
        .OrderByDescending(m => m.SentAt)
        .Select(m => m.SentAt)
        .FirstOrDefault()

            }).ToList();
        }
    }
}
