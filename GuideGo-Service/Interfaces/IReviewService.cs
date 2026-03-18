using GuideGo_Service.Dtos.Review;

namespace GuideGo_Service.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetAllAsync();
    Task<ReviewResponseDto?> GetByIdAsync(Guid reviewId);
    Task<(bool Success, string Message)> CreateAsync(CreateReviewRequestDto request, Guid userId);
    Task<(bool Success, string Message)> UpdateAsync(Guid reviewId, UpdateReviewRequestDto request, Guid userId, bool isAdmin = false);
    Task<(bool Success, string Message)> DeleteAsync(Guid reviewId, Guid userId, bool isAdmin = false);
}
