using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Cart;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartResponseDto> GetMyCartAsync(Guid userId)
    {
        var cart = await EnsureCartExistsAsync(userId);

        return MapToCartResponse(cart);
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(Guid userId, AddToCartRequestDto request)
    {
        var cart = await EnsureCartExistsAsync(userId);

        var tour = await _cartRepository.GetActiveTourByIdAsync(request.TourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour hoặc tour đã ngừng hoạt động.");
        }

        var schedule = await _cartRepository.GetScheduleForTourAsync(request.TourId, request.ScheduleId);
        if (schedule is null)
        {
            return (false, "Không tìm thấy lịch khởi hành phù hợp cho tour.");
        }

        if (schedule.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return (false, "Không thể thêm lịch đã hết hạn vào giỏ hàng.");
        }

        if (request.PeopleCount > schedule.AvailableSlots)
        {
            return (false, "Số lượng người vượt quá số chỗ còn lại.");
        }

        var existingItem = await _cartRepository.GetCartItemAsync(cart.Id, request.TourId, request.ScheduleId);
        if (existingItem is null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                TourId = request.TourId,
                ScheduleId = request.ScheduleId,
                PeopleCount = request.PeopleCount,
                CreatedAt = DateTime.UtcNow
            };

            await _cartRepository.AddCartItemAsync(item);
        }
        else
        {
            var updatedPeopleCount = existingItem.PeopleCount + request.PeopleCount;
            if (updatedPeopleCount > schedule.AvailableSlots)
            {
                return (false, "Tổng số người trong giỏ vượt quá số chỗ còn lại.");
            }

            existingItem.PeopleCount = updatedPeopleCount;
            _cartRepository.UpdateCartItem(existingItem);
        }

        await _cartRepository.SaveChangesAsync();

        return (true, "Thêm vào giỏ hàng thành công.");
    }

    public async Task<(bool Success, string Message)> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart is null)
        {
            return (false, "Không tìm thấy giỏ hàng.");
        }

        var item = await _cartRepository.GetCartItemByIdAsync(cartItemId);
        if (item is null || item.CartId != cart.Id)
        {
            return (false, "Không tìm thấy item trong giỏ hàng.");
        }

        _cartRepository.RemoveCartItem(item);
        await _cartRepository.SaveChangesAsync();

        return (true, "Item removed from cart successfully.");
    }

    private async Task<Cart> EnsureCartExistsAsync(Guid userId)
    {
        var existingCart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (existingCart is not null)
        {
            return existingCart;
        }

        var cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _cartRepository.CreateCartAsync(cart);

        return (await _cartRepository.GetCartByUserIdAsync(userId))!;
    }

    private static CartResponseDto MapToCartResponse(Cart cart)
    {
        var items = cart.Items.Select(item => new CartItemResponseDto
        {
            Id = item.Id,
            TourId = item.TourId,
            TourTitle = item.Tour.Title,
            ScheduleId = item.ScheduleId,
            StartDate = item.Schedule.StartDate,
            EndDate = item.Schedule.EndDate,
            PeopleCount = item.PeopleCount,
            PricePerPerson = item.Tour.PricePerPerson,
            LineTotal = item.PeopleCount * item.Tour.PricePerPerson
        }).ToList();

        return new CartResponseDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            Items = items,
            TotalAmount = items.Sum(item => item.LineTotal)
        };
    }
}