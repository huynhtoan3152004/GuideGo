using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces
{
    public interface IChatRepository
    {
        Task<Chat?> GetChatAsync(Guid userId, Guid guideId);
        Task<Chat?> GetByIdAsync(Guid chatId);
        Task<Chat> CreateChatAsync(Chat chat);
        Task<List<Message>> GetMessagesAsync(Guid chatId);
        Task AddMessageAsync(Message message);
        Task<List<Chat>> GetMyChats(Guid userId, Guid? guideId);
    }
}
