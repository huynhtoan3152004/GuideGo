using GuideGo_Service.Dtos.Tour;

namespace GuideGo_Service.Interfaces;

public interface ITourService
{
    Task<IEnumerable<TourResponseDto>> GetAllActiveToursAsync();
    Task<IEnumerable<TourResponseDto>> GetMyRequestedToursAsync(Guid userId);
    Task<IEnumerable<TourResponseDto>> GetGuideRequestsAsync(Guid userId);
    Task<TourSearchResultDto> SearchToursAsync(SearchToursRequestDto request);
    Task<IEnumerable<SuitableGuideDto>> GetSuitableGuidesAsync(Guid locationId, string? language, bool verifiedOnly, int limit);
    Task<TourResponseDto?> GetTourByIdAsync(Guid id);
    Task<(bool Success, string Message, TourResponseDto? Data)> CreateTourAsync(CreateTourRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message, TourResponseDto? Data)> CreateUserTourRequestAsync(CreateUserTourRequestDto request, Guid actorId);
    Task<(bool Success, string Message)> AssignGuideToRequestedTourAsync(Guid tourId, Guid actorId, Guid guideId);
    Task<(bool Success, string Message)> RespondToRequestedTourAsync(Guid tourId, Guid actorId, bool accept);
    Task<(bool Success, string Message)> UpdateTourAsync(Guid id, UpdateTourRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> CreateScheduleAsync(CreateTourScheduleRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> UpdateScheduleAsync(Guid tourId, Guid scheduleId, UpdateTourScheduleRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> DeleteTourAsync(Guid id, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> AddTourImageAsync(Guid tourId, Guid actorId, bool isAdmin, string imageUrl);
}