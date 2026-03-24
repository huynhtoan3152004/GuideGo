using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GuideGo_Service.Dtos.Tour;

/// <summary>
/// Dữ liệu đầu vào khi cập nhật tour.
/// </summary>
public class UpdateTourRequestDto
{
    /// <summary>
    /// Tiêu đề tour.
    /// </summary>
    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự.")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết tour.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Id địa điểm tổ chức tour.
    /// </summary>
    [Required(ErrorMessage = "Vị trí là bắt buộc.")]
    [JsonPropertyName("location_id")]
    public Guid LocationId { get; set; }

    /// <summary>
    /// Id hướng dẫn viên. Admin có thể chỉ định.
    /// </summary>
    [JsonPropertyName("guide_id")]
    public Guid? GuideId { get; set; }

    /// <summary>
    /// Giá tiền cho mỗi người tham gia.
    /// </summary>
    [Required(ErrorMessage = "Giá mỗi người là bắt buộc.")]
    [Range(0.01, 9999999999.0, ErrorMessage = "Giá mỗi người phải lớn hơn 0.")]
    [JsonPropertyName("price_per_person")]
    public decimal PricePerPerson { get; set; }

    /// <summary>
    /// Số lượng người tham gia tối đa.
    /// </summary>
    [Required(ErrorMessage = "Số người tối đa là bắt buộc.")]
    [Range(1, 1000, ErrorMessage = "Số người tối đa phải ít nhất là 1.")]
    [JsonPropertyName("max_people")]
    public int MaxPeople { get; set; }

    /// <summary>
    /// Số ngày diễn ra tour.
    /// </summary>
    [Required(ErrorMessage = "Số ngày tour là bắt buộc.")]
    [Range(1, 365, ErrorMessage = "Số ngày tour phải ít nhất là 1.")]
    [JsonPropertyName("duration_days")]
    public int DurationDays { get; set; }
}