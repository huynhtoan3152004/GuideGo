using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class SuitableGuideDto
{
    [JsonPropertyName("guide_id")]
    public Guid GuideId { get; set; }

    [JsonPropertyName("guide_name")]
    public string GuideName { get; set; } = string.Empty;

    [JsonPropertyName("languages")]
    public IEnumerable<string> Languages { get; set; } = [];

    [JsonPropertyName("experience_years")]
    public int ExperienceYears { get; set; }

    [JsonPropertyName("rating")]
    public decimal Rating { get; set; }

    [JsonPropertyName("is_verified")]
    public bool IsVerified { get; set; }
}
