using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

public class CreateUserTourRequestDto
{
    [Required(ErrorMessage = "title là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "title tối đa 200 ký tự.")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "location_id là bắt buộc.")]
    [JsonPropertyName("location_id")]
    public Guid LocationId { get; set; }

    [Required(ErrorMessage = "start_date là bắt buộc.")]
    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "end_date là bắt buộc.")]
    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [Range(1, 1000, ErrorMessage = "people_count phải lớn hơn 0.")]
    [JsonPropertyName("people_count")]
    public int PeopleCount { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "budget_per_person phải lớn hơn 0.")]
    [JsonPropertyName("budget_per_person")]
    public decimal BudgetPerPerson { get; set; }

    [JsonPropertyName("preferred_guide_id")]
    public Guid? PreferredGuideId { get; set; }
}
