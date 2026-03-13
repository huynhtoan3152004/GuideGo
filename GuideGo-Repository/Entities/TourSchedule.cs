using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("tour_schedules")]
public class TourSchedule
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int AvailableSlots { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tour Tour { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
