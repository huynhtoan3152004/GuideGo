namespace GuideGo_Service.Dtos.Chat
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GuideId { get; set; }

        public string UserName { get; set; } = null!;
        public string GuideName { get; set; } = null!;

        public string? LastMessage { get; set; }
        public DateTime? LastTime { get; set; }
    }
}

