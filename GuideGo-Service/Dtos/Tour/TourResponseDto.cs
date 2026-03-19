using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class TourResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location_id")]
    public Guid LocationId { get; set; }

    [JsonPropertyName("location_name")]
    public string? LocationName { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("location")]
    public TourLocationDto? Location { get; set; }

    [JsonPropertyName("guide_id")]
    public Guid? GuideId { get; set; }

    [JsonPropertyName("guide_name")]
    public string? GuideName { get; set; }

    [JsonPropertyName("guide_experience_years")]
    public int? GuideExperienceYears { get; set; }

    [JsonPropertyName("guide_languages")]
    public IEnumerable<string> GuideLanguages { get; set; } = [];

    [JsonPropertyName("guide_is_verified")]
    public bool GuideIsVerified { get; set; }

    [JsonPropertyName("price_per_person")]
    public decimal PricePerPerson { get; set; }

    [JsonPropertyName("max_people")]
    public int MaxPeople { get; set; }

    [JsonPropertyName("duration_days")]
    public int DurationDays { get; set; }

    [JsonPropertyName("rating")]
    public decimal Rating { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("image_urls")]
    public IEnumerable<string> ImageUrls { get; set; } = [];

    [JsonPropertyName("schedules")]
    public IEnumerable<TourScheduleResponseDto> Schedules { get; set; } = [];
}