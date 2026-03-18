using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await _dbSet.FirstOrDefaultAsync(user => user.Phone == phone);
    }
}
