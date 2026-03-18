namespace GuideGo_Repository.DTOs;

public class PaymentCreateDto
{
    public List<Guid> BookingIds { get; set; } = [];
    public string PaymentMethod { get; set; } = null!;
}
