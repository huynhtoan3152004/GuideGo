using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.User;

public class UserResponseDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
