using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("tour_images")]
public class TourImage
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tour Tour { get; set; } = null!;
}
