using GuideGo_Service.Dtos.Tour;

namespace GuideGo_Service.Interfaces;

public interface ITourService
{
    Task<IEnumerable<TourResponseDto>> GetAllActiveToursAsync();
    Task<TourResponseDto?> GetTourByIdAsync(Guid id);
    Task<(bool Success, string Message, TourResponseDto? Data)> CreateTourAsync(CreateTourRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> UpdateTourAsync(Guid id, UpdateTourRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> DeleteTourAsync(Guid id, Guid actorId, bool isAdmin);
}