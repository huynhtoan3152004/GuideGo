using GuideGo_Repository.Data;
using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Tour)
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Schedule)
            .FirstOrDefaultAsync(cart => cart.UserId == userId);
    }

    public async Task<Cart> CreateCartAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Tour?> GetActiveTourByIdAsync(Guid tourId)
    {
        return await _context.Tours
            .AsNoTracking()
            .FirstOrDefaultAsync(tour => tour.Id == tourId && tour.IsActive);
    }

    public async Task<TourSchedule?> GetScheduleForTourAsync(Guid tourId, Guid scheduleId)
    {
        return await _context.TourSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(schedule => schedule.Id == scheduleId && schedule.TourId == tourId);
    }

    public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid tourId, Guid scheduleId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(item =>
                item.CartId == cartId &&
                item.TourId == tourId &&
                item.ScheduleId == scheduleId);
    }

    public async Task AddCartItemAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
    }

    public void UpdateCartItem(CartItem item)
    {
        _context.CartItems.Update(item);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}