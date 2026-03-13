using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("chats")]
public class Chat
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GuideId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Guide Guide { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = [];
}
