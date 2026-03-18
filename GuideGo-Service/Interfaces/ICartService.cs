using GuideGo_Service.Dtos.Cart;

namespace GuideGo_Service.Interfaces;

public interface ICartService
{
    Task<CartResponseDto> GetMyCartAsync(Guid userId);
    Task<(bool Success, string Message)> AddToCartAsync(Guid userId, AddToCartRequestDto request);
}