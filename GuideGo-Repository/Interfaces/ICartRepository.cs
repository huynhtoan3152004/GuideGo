using GuideGo_Repository.Entities;

namespace GuideGo_Repository.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(Guid userId);
    Task<Cart> CreateCartAsync(Cart cart);
    Task<Tour?> GetActiveTourByIdAsync(Guid tourId);
    Task<TourSchedule?> GetScheduleForTourAsync(Guid tourId, Guid scheduleId);
    Task<CartItem?> GetCartItemAsync(Guid cartId, Guid tourId, Guid scheduleId);
    Task AddCartItemAsync(CartItem item);
    void UpdateCartItem(CartItem item);
    Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId);
    void RemoveCartItem(CartItem item);
    Task<int> SaveChangesAsync();
}