namespace GuideGo_Repository.DTOs;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
}
