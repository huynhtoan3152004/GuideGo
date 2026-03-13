using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("reviews")]
public class Review
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tour Tour { get; set; } = null!;
    public User User { get; set; } = null!;
}
