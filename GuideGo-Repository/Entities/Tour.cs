using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("tours")]
public class Tour
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }
    public Guid LocationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? GuideId { get; set; }
    public decimal PricePerPerson { get; set; }
    public int MaxPeople { get; set; }
    public int DurationDays { get; set; }
    public decimal Rating { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Location Location { get; set; } = null!;
    public Company? Company { get; set; }
    public Guide? Guide { get; set; }
    public ICollection<TourImage> Images { get; set; } = [];
    public ICollection<TourSchedule> Schedules { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
}
