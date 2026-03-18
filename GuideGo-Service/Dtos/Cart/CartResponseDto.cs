using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Cart;

public class CartResponseDto
{
    [JsonPropertyName("cart_id")]
    public Guid CartId { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("items")]
    public IEnumerable<CartItemResponseDto> Items { get; set; } = [];

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }
}