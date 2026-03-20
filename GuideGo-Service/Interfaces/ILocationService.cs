using GuideGo_Service.Dtos.Location;

namespace GuideGo_Service.Interfaces;

public interface ILocationService
{
    Task<IEnumerable<LocationDto>> GetAllLocationsAsync();
    Task<LocationDto?> GetLocationByIdAsync(Guid id);
    Task<LocationDto> CreateLocationAsync(CreateLocationDto dto);
    Task<bool> UpdateLocationAsync(Guid id, UpdateLocationDto dto);
    Task<bool> DeleteLocationAsync(Guid id);
}