using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Cart;

public class CartItemResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("tour_id")]
    public Guid TourId { get; set; }

    [JsonPropertyName("tour_title")]
    public string TourTitle { get; set; } = string.Empty;

    [JsonPropertyName("schedule_id")]
    public Guid ScheduleId { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [JsonPropertyName("people_count")]
    public int PeopleCount { get; set; }

    [JsonPropertyName("price_per_person")]
    public decimal PricePerPerson { get; set; }

    [JsonPropertyName("line_total")]
    public decimal LineTotal { get; set; }
}