using GuideGo_Service.Dtos.Tour;

namespace GuideGo_Service.Interfaces;

public interface ITourScheduleService
{
    Task<IEnumerable<TourScheduleResponseDto>> GetByTourIdAsync(Guid tourId);
    Task<TourScheduleResponseDto?> GetByIdAsync(Guid id);
    Task<(bool Success, string Message, TourScheduleResponseDto? Data)> CreateAsync(CreateTourScheduleRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, UpdateTourScheduleRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid actorId, bool isAdmin);
}
