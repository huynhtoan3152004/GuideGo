namespace GuideGo_Service.Dtos.Chat
{
    public class SendMessageRequest
    {
        public Guid ChatId { get; set; }
        public string Content { get; set; } = null!;
    }
}
