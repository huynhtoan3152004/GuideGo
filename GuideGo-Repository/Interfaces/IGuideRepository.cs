using GuideGo_Repository.Entities;
using GuideGo_Repository.Repositories.Interfaces;

namespace GuideGo_Repository.Interfaces
{
    public interface IGuideRepository : IGenericRepository<Guide>
    {
        Task<IEnumerable<Guide>> GetAllWithUserAsync();
        Task<Guide?> GetByUserIdAsync(Guid userId);
    }
}
