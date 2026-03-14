namespace GuideGo_Repository.DTOs;

public class PaymentCreateDto
{
    public Guid BookingId { get; set; }
    public string PaymentMethod { get; set; } = null!;
}
