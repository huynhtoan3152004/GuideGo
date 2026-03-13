using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("guides")]
public class Guide
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int ExperienceYears { get; set; }
    public string[] Languages { get; set; } = [];
    public string? Description { get; set; }
    public decimal Rating { get; set; }
    public bool IsVerified { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Tour> Tours { get; set; } = [];
    public ICollection<Chat> Chats { get; set; } = [];
}
