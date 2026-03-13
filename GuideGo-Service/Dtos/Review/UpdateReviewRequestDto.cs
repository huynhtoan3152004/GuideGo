using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Review;

public class UpdateReviewRequestDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
