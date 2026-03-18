namespace GuideGo_Service.Dtos.Guide
{
    public class CreateGuideDto
    {
        public Guid UserId { get; set; }
        public int ExperienceYears { get; set; }
        public string[] Languages { get; set; } = [];
        public string? Description { get; set; }
    }
}
