using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuideGo_Repository.Enums;

namespace GuideGo_Repository.Entities;

[Table("bookings")]
public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScheduleId { get; set; }
    public int PeopleCount { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? PaymentId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public TourSchedule Schedule { get; set; } = null!;
    public Payment? Payment { get; set; }
}
