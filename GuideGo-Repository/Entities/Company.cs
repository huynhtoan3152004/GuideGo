using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("companies")]
public class Company
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string CompanyName { get; set; } = null!;

    public string? Address { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Tour> Tours { get; set; } = [];
}
