using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class TourScheduleResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [JsonPropertyName("available_slots")]
    public int AvailableSlots { get; set; }
}