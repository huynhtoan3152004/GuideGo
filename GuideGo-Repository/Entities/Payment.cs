using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuideGo_Repository.Enums;

namespace GuideGo_Repository.Entities;

[Table("payments")]
public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = null!;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = [];
}
