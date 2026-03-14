using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Review;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetMyReviewsAsync(Guid userId)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId);
        return reviews.Select(MapToDto);
    }

    public async Task<ReviewResponseDto?> GetMyReviewByIdAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepository.GetByIdAndUserIdAsync(reviewId, userId);
        return review is null ? null : MapToDto(review);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateReviewRequestDto request, Guid userId)
    {
        var tourIsValidForGuideReview = await _reviewRepository.TourExistsWithGuideAsync(request.TourId);
        if (!tourIsValidForGuideReview)
        {
            return (false, "Create review unsuccessfully. Tour not found or has no guide.");
        }

        var review = new Review
        {
            TourId = request.TourId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        await _reviewRepository.RecalculateGuideAverageRatingByTourIdAsync(review.TourId);
        await _reviewRepository.SaveChangesAsync();

        return (true, "Create review successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid reviewId, UpdateReviewRequestDto request, Guid userId)
    {
        var review = await _reviewRepository.GetByIdAndUserIdAsync(reviewId, userId);
        if (review is null)
        {
            return (false, "Review not found.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment?.Trim();

        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await _reviewRepository.RecalculateGuideAverageRatingByTourIdAsync(review.TourId);
        await _reviewRepository.SaveChangesAsync();

        return (true, "Update review successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepository.GetByIdAndUserIdAsync(reviewId, userId);
        if (review is null)
        {
            return (false, "Review not found.");
        }

        var tourId = review.TourId;

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync();

        await _reviewRepository.RecalculateGuideAverageRatingByTourIdAsync(tourId);
        await _reviewRepository.SaveChangesAsync();

        return (true, "Delete review successfully.");
    }

    private static ReviewResponseDto MapToDto(Review review)
    {
        return new ReviewResponseDto
        {
            Id = review.Id,
            TourId = review.TourId,
            UserId = review.UserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
