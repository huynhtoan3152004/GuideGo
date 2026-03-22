using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class GuideDecisionRequestDto
{
    [Required(ErrorMessage = "accept là bắt buộc.")]
    [JsonPropertyName("accept")]
    public bool Accept { get; set; }
}
