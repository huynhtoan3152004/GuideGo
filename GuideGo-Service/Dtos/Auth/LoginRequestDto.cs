using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
