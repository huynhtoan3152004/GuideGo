using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Chat?> GetChatAsync(Guid userId, Guid guideId)
        {
            return await _context.Chats
                .FirstOrDefaultAsync(c =>
                c.UserId == userId && c.GuideId == guideId
    );
        }

        public async Task<Chat?> GetByIdAsync(Guid chatId)
        {
            return await _context.Chats.FindAsync(chatId);
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task<List<Message>> GetMessagesAsync(Guid chatId)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task AddMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Chat>> GetMyChats(Guid userId, Guid? guideId)
        {
            return await _context.Chats
                .Include(c => c.User)
                .Include(c => c.Guide)
                .ThenInclude(g => g.User)
                .Include(c => c.Messages) 
                .Where(c =>
                     c.UserId == userId ||
                    (guideId != null && c.GuideId == guideId)
        )
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
