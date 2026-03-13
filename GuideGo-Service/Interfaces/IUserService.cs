using GuideGo_Service.Dtos.User;

namespace GuideGo_Service.Interfaces;

public interface IUserService
{
    Task EnsureAdminAccountAsync();
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<(bool Success, string Message)> CreateGuideUserAsync(CreateUserRequestDto request);
    Task<(bool Success, string Message)> UpdateUserAsync(Guid id, UpdateUserRequestDto request, Guid actorId, bool isAdmin);
    Task<(bool Success, string Message)> SoftDeleteAsync(Guid id);
}
