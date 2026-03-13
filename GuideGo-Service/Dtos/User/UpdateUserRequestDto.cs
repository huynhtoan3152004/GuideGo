using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.User;

public class UpdateUserRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression("^\\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}
