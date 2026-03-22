using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class CreateTourScheduleRequestDto
{
    [Required(ErrorMessage = "tour_id là bắt buộc.")]
    [JsonPropertyName("tour_id")]
    public Guid TourId { get; set; }

    [Required(ErrorMessage = "start_date là bắt buộc.")]
    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "end_date là bắt buộc.")]
    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "available_slots phải lớn hơn 0.")]
    [JsonPropertyName("available_slots")]
    public int AvailableSlots { get; set; }
}
