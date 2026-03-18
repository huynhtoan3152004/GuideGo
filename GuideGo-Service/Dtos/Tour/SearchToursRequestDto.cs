using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

/// <summary>
/// Bộ lọc tìm kiếm tour.
/// </summary>
public class SearchToursRequestDto
{
    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("location_id")]
    public Guid? LocationId { get; set; }

    [JsonPropertyName("guide_language")]
    public string? GuideLanguage { get; set; }

    [JsonPropertyName("verified_guide_only")]
    public bool VerifiedGuideOnly { get; set; }

    [JsonPropertyName("min_price")]
    public decimal? MinPrice { get; set; }

    [JsonPropertyName("max_price")]
    public decimal? MaxPrice { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly? EndDate { get; set; }

    [JsonPropertyName("sort_by")]
    public string? SortBy { get; set; } = "created_at";

    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; } = 20;
}