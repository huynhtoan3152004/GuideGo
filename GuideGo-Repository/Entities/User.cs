using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GuideGo_Repository.Enums;

namespace GuideGo_Repository.Entities;

[Table("users")]
public class User
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [MaxLength(150)]
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public UserRole Role { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Guide? Guide { get; set; }
    public Company? Company { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Chat> Chats { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
}
