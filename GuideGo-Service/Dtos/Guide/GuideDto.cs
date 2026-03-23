namespace GuideGo_Service.Dtos.Guide
{
    public class GuideDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int ExperienceYears { get; set; }
        public string FullName { get; set; } = null!;
        public string[] Languages { get; set; } = [];
        public string? Description { get; set; }
        public decimal Rating { get; set; }
        public bool IsVerified { get; set; }
    }
}
