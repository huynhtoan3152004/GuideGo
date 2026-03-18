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

    public async Task<IEnumerable<ReviewResponseDto>> GetAllAsync()
    {
        var reviews = await _reviewRepository.GetAllWithDetailsAsync();
        return reviews.Select(MapToDto);
    }

    public async Task<ReviewResponseDto?> GetByIdAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdWithDetailsAsync(reviewId);
        return review is null ? null : MapToDto(review);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateReviewRequestDto request, Guid userId)
    {
        var tourIsValidForGuideReview = await _reviewRepository.TourExistsWithGuideAsync(request.TourId);
        if (!tourIsValidForGuideReview)
        {
            return (false, "Create review unsuccessfully. Tour not found or has no guide.");
        }

        // Check if user has a confirmed booking and finished the tour schedule
        var userHasCompletedBooking = await _reviewRepository.UserHasCompletedBookingForTourAsync(userId, request.TourId);
        if (!userHasCompletedBooking)
        {
            return (false, "Create review unsuccessfully. You can only review tours you completed with confirmed booking.");
        }

        // Check if user already reviewed this tour (prevent spam)
        var userAlreadyReviewed = await _reviewRepository.UserAlreadyReviewedTourAsync(userId, request.TourId);
        if (userAlreadyReviewed)
        {
            return (false, "Create review unsuccessfully. You have already reviewed this tour.");
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

    public async Task<(bool Success, string Message)> UpdateAsync(Guid reviewId, UpdateReviewRequestDto request, Guid userId, bool isAdmin = false)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review is null)
        {
            return (false, "Review not found.");
        }

        // Check if user is author or admin
        if (review.UserId != userId && !isAdmin)
        {
            return (false, "You are not authorized to update this review.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment?.Trim();

        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await _reviewRepository.RecalculateGuideAverageRatingByTourIdAsync(review.TourId);
        await _reviewRepository.SaveChangesAsync();

        return (true, "Update review successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid reviewId, Guid userId, bool isAdmin = false)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review is null)
        {
            return (false, "Review not found.");
        }

        // Check if user is author or admin
        if (review.UserId != userId && !isAdmin)
        {
            return (false, "You are not authorized to delete this review.");
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
            FullName = review.User?.FullName,
            TourTitle = review.Tour?.Title,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
