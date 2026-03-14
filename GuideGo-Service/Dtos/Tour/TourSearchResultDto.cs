using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

/// <summary>
/// Kết quả tìm kiếm tour có phân trang.
/// </summary>
public class TourSearchResultDto
{
    [JsonPropertyName("items")]
    public IEnumerable<TourResponseDto> Items { get; set; } = [];

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}