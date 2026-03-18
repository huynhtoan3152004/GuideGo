using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string phone);
    Task AddAsync(User user);
    void Update(User user);
    Task<int> SaveChangesAsync();
}
