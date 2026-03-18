using GuideGo_Service.Dtos.Chat;

namespace GuideGo_Service.Interfaces
{
    public interface IChatService
    {
        Task<ChatDto> GetOrCreateChat(Guid userId, Guid guideId);
        Task<List<MessageDto>> GetMessages(Guid chatId, Guid userId);
        Task<MessageDto> SendMessage(Guid chatId, Guid senderId, string content);
        Task<List<ChatDto>> GetMyChats(Guid userId);
    }
}
