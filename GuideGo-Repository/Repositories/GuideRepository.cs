using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories
{
    public class GuideRepository : GenericRepository<Guide>, IGuideRepository
    {
        public GuideRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Guide>> GetAllWithUserAsync()
        {
            return await _dbSet
                .Include(g => g.User)
                .ToListAsync();
        }
        public async Task<Guide?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.UserId == userId);
        }
    }
}
