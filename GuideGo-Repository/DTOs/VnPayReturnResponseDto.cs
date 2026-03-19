namespace GuideGo_Repository.DTOs;

public class VnPayReturnResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string VnPayTransactionId { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
}
