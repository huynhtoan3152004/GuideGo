using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Cart;

/// <summary>
/// Dữ liệu thêm tour vào giỏ hàng.
/// </summary>
public class AddToCartRequestDto
{
    [Required(ErrorMessage = "Tour là bắt buộc.")]
    [JsonPropertyName("tour_id")]
    public Guid TourId { get; set; }

    [Required(ErrorMessage = "Lịch tour là bắt buộc.")]
    [JsonPropertyName("schedule_id")]
    public Guid ScheduleId { get; set; }

    [Required(ErrorMessage = "Số người là bắt buộc.")]
    [Range(1, 1000, ErrorMessage = "Số người phải lớn hơn 0.")]
    [JsonPropertyName("people_count")]
    public int PeopleCount { get; set; }
}