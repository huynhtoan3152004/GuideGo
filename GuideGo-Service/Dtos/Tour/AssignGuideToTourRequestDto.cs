using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class AssignGuideToTourRequestDto
{
    [Required(ErrorMessage = "guide_id là bắt buộc.")]
    [JsonPropertyName("guide_id")]
    public Guid GuideId { get; set; }
}
