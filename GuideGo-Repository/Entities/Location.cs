using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuideGo_Repository.Entities;

[Table("locations")]
public class Location
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = null!;

    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    // Navigation
    public ICollection<Tour> Tours { get; set; } = [];
}
