using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("cart_items")]
public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid TourId { get; set; }
    public Guid ScheduleId { get; set; }
    public int PeopleCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Cart Cart { get; set; } = null!;
    public Tour Tour { get; set; } = null!;
    public TourSchedule Schedule { get; set; } = null!;
}
