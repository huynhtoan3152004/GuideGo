using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Location;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;

    public LocationService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
    {
        var locations = await _locationRepository.GetAllAsync();
        return locations.Select(MapToDto);
    }

    public async Task<LocationDto?> GetLocationByIdAsync(Guid id)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        return location is null ? null : MapToDto(location);
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationDto dto)
    {
        var location = new Location
        {
            Name = dto.Name.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address?.Trim(),
            City = dto.City?.Trim(),
            Country = dto.Country?.Trim()
        };

        await _locationRepository.AddAsync(location);
        await _locationRepository.SaveChangesAsync();

        return MapToDto(location);
    }

    public async Task<bool> UpdateLocationAsync(Guid id, UpdateLocationDto dto)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            location.Name = dto.Name.Trim();
        }

        if (dto.Latitude.HasValue)
        {
            location.Latitude = dto.Latitude.Value;
        }

        if (dto.Longitude.HasValue)
        {
            location.Longitude = dto.Longitude.Value;
        }

        if (dto.Address is not null)
        {
            location.Address = dto.Address.Trim();
        }

        if (dto.City is not null)
        {
            location.City = dto.City.Trim();
        }

        if (dto.Country is not null)
        {
            location.Country = dto.Country.Trim();
        }

        _locationRepository.Update(location);
        await _locationRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteLocationAsync(Guid id)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location is null)
        {
            return false;
        }

        _locationRepository.Remove(location);
        await _locationRepository.SaveChangesAsync();
        return true;
    }

    private static LocationDto MapToDto(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Address = location.Address,
            City = location.City,
            Country = location.Country
        };
    }
}