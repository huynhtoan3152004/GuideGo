using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Tour;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;

    public TourService(ITourRepository tourRepository)
    {
        _tourRepository = tourRepository;
    }

    public async Task<IEnumerable<TourResponseDto>> GetAllActiveToursAsync()
    {
        var tours = await _tourRepository.GetAllActiveToursAsync();
        return tours.Select(MapToDto);
    }

    public async Task<TourSearchResultDto> SearchToursAsync(SearchToursRequestDto request)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var (items, totalItems) = await _tourRepository.SearchActiveToursAsync(
            request.Keyword,
            request.City,
            request.LocationId,
            request.GuideLanguage,
            request.VerifiedGuideOnly,
            request.MinPrice,
            request.MaxPrice,
            request.StartDate,
            request.EndDate,
            request.SortBy,
            page,
            pageSize);

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new TourSearchResultDto
        {
            Items = items.Select(MapToDto),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<TourResponseDto?> GetTourByIdAsync(Guid id)
    {
        var tour = await _tourRepository.GetTourByIdAsync(id);
        return tour is null ? null : MapToDto(tour);
    }

    public async Task<(bool Success, string Message, TourResponseDto? Data)> CreateTourAsync(CreateTourRequestDto request, Guid actorId, bool isAdmin)
    {
        if (!await _tourRepository.ExistsLocationAsync(request.LocationId))
        {
            return (false, "Tạo tour thất bại. Không tìm thấy địa điểm.", null);
        }

        var guideIdResult = await ResolveGuideIdAsync(request.GuideId, actorId, isAdmin);
        if (!guideIdResult.Success)
        {
            return (false, guideIdResult.Message, null);
        }

        var tour = new Tour
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            LocationId = request.LocationId,
            GuideId = guideIdResult.GuideId,
            PricePerPerson = request.PricePerPerson,
            MaxPeople = request.MaxPeople,
            DurationDays = request.DurationDays,
            Rating = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tourRepository.AddTourAsync(tour);

        var createdTour = await _tourRepository.GetTourByIdAsync(tour.Id);
        var response = createdTour is null ? null : MapToDto(createdTour);

        return (true, "Tạo tour thành công.", response);
    }

    public async Task<(bool Success, string Message)> UpdateTourAsync(Guid id, UpdateTourRequestDto request, Guid actorId, bool isAdmin)
    {
        var tour = await _tourRepository.GetTourByIdAsync(id);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        if (await _tourRepository.HasActiveBookingsAsync(id))
        {
            return (false, "Không thể cập nhật tour vì đang có đơn đặt chỗ hoạt động.");
        }

        if (!await _tourRepository.ExistsLocationAsync(request.LocationId))
        {
            return (false, "Cập nhật tour thất bại. Không tìm thấy địa điểm.");
        }

        var guideIdResult = await ResolveGuideIdAsync(request.GuideId, actorId, isAdmin, keepCurrentGuideId: tour.GuideId);
        if (!guideIdResult.Success)
        {
            return (false, guideIdResult.Message);
        }

        tour.Title = request.Title.Trim();
        tour.Description = request.Description?.Trim();
        tour.LocationId = request.LocationId;
        tour.GuideId = guideIdResult.GuideId;
        tour.PricePerPerson = request.PricePerPerson;
        tour.MaxPeople = request.MaxPeople;
        tour.DurationDays = request.DurationDays;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateTourAsync(tour);

        return (true, "Cập nhật tour thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteTourAsync(Guid id, Guid actorId, bool isAdmin)
    {
        var tour = await _tourRepository.GetTourByIdAsync(id);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        if (await _tourRepository.HasActiveBookingsAsync(id))
        {
            return (false, "Không thể xóa tour vì đang có đơn đặt chỗ hoạt động.");
        }

        tour.IsActive = false;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateTourAsync(tour);

        return (true, "Xóa tour thành công.");
    }

    public async Task<(bool Success, string Message)> AddTourImageAsync(Guid tourId, Guid actorId, bool isAdmin, string imageUrl)
    {
        var tour = await _tourRepository.GetTourByIdAsync(tourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        await _tourRepository.AddTourImageAsync(new TourImage
        {
            TourId = tourId,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        });

        return (true, "Upload ảnh tour thành công.");
    }

    private async Task<(bool Success, string Message)> CheckTourPermissionAsync(Tour tour, Guid actorId, bool isAdmin)
    {
        if (isAdmin)
        {
            return (true, string.Empty);
        }

        var actorGuideId = await _tourRepository.GetGuideIdByUserIdAsync(actorId);
        if (!actorGuideId.HasValue)
        {
            return (false, "Bạn không có quyền quản lý tour.");
        }

        var isOwner = await _tourRepository.IsTourOwnedByGuideAsync(tour.Id, actorGuideId.Value);
        if (!isOwner)
        {
            return (false, "Bạn không có quyền quản lý tour này.");
        }

        return (true, string.Empty);
    }

    private async Task<(bool Success, string Message, Guid? GuideId)> ResolveGuideIdAsync(Guid? requestedGuideId, Guid actorId, bool isAdmin, Guid? keepCurrentGuideId = null)
    {
        if (!isAdmin)
        {
            var actorGuideId = await _tourRepository.GetGuideIdByUserIdAsync(actorId);
            if (!actorGuideId.HasValue)
            {
                return (false, "Tạo/Cập nhật tour thất bại. Không tìm thấy hồ sơ hướng dẫn viên.", null);
            }

            return (true, string.Empty, actorGuideId.Value);
        }

        if (requestedGuideId.HasValue)
        {
            var guideExists = await _tourRepository.ExistsGuideAsync(requestedGuideId.Value);
            if (!guideExists)
            {
                return (false, "Tạo/Cập nhật tour thất bại. Không tìm thấy hướng dẫn viên.", null);
            }

            return (true, string.Empty, requestedGuideId.Value);
        }

        if (keepCurrentGuideId.HasValue)
        {
            return (true, string.Empty, keepCurrentGuideId.Value);
        }

        return (false, "Tạo tour thất bại. Admin phải truyền guide_id.", null);
    }

    private static TourResponseDto MapToDto(Tour tour)
    {
        return new TourResponseDto
        {
            Id = tour.Id,
            Title = tour.Title,
            Description = tour.Description,
            LocationId = tour.LocationId,
            LocationName = tour.Location?.Name,
            City = tour.Location?.City,
            Latitude = tour.Location?.Latitude,
            Longitude = tour.Location?.Longitude,
            Location = tour.Location is null
                ? null
                : new TourLocationDto
                {
                    Id = tour.Location.Id,
                    Name = tour.Location.Name,
                    Address = tour.Location.Address,
                    City = tour.Location.City,
                    Country = tour.Location.Country,
                    Latitude = tour.Location.Latitude,
                    Longitude = tour.Location.Longitude
                },
            GuideId = tour.GuideId,
            GuideName = tour.Guide?.User?.FullName,
            GuideExperienceYears = tour.Guide?.ExperienceYears,
            GuideLanguages = tour.Guide?.Languages ?? [],
            GuideIsVerified = tour.Guide?.IsVerified ?? false,
            PricePerPerson = tour.PricePerPerson,
            MaxPeople = tour.MaxPeople,
            DurationDays = tour.DurationDays,
            Rating = tour.Rating,
            IsActive = tour.IsActive,
            CreatedAt = tour.CreatedAt,
            UpdatedAt = tour.UpdatedAt,
            ImageUrls = tour.Images.Select(image => image.ImageUrl),
            Schedules = tour.Schedules.Select(schedule => new TourScheduleResponseDto
            {
                Id = schedule.Id,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                AvailableSlots = schedule.AvailableSlots
            })
        };
    }
}