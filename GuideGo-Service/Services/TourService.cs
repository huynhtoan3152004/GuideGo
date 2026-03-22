using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
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

    public async Task<IEnumerable<TourResponseDto>> GetMyRequestedToursAsync(Guid userId)
    {
        var tours = await _tourRepository.GetMyRequestedToursAsync(userId);
        return tours.Select(MapToDto);
    }

    public async Task<IEnumerable<SuitableGuideDto>> GetSuitableGuidesAsync(Guid locationId, string? language, bool verifiedOnly, int limit)
    {
        if (!await _tourRepository.ExistsLocationAsync(locationId))
        {
            return [];
        }

        var top = limit <= 0 ? 20 : Math.Min(limit, 100);
        var guides = await _tourRepository.FindSuitableGuidesAsync(locationId, language, verifiedOnly, top);

        return guides.Select(guide => new SuitableGuideDto
        {
            GuideId = guide.Id,
            GuideName = guide.User.FullName,
            Languages = guide.Languages,
            ExperienceYears = guide.ExperienceYears,
            Rating = guide.Rating,
            IsVerified = guide.IsVerified
        });
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

    public async Task<(bool Success, string Message, TourResponseDto? Data)> CreateUserTourRequestAsync(CreateUserTourRequestDto request, Guid actorId)
    {
        if (!await _tourRepository.ExistsUserAsync(actorId))
        {
            return (false, "Không tìm thấy người dùng.", null);
        }

        if (!await _tourRepository.ExistsLocationAsync(request.LocationId))
        {
            return (false, "Không tìm thấy địa điểm.", null);
        }

        if (request.EndDate < request.StartDate)
        {
            return (false, "end_date phải lớn hơn hoặc bằng start_date.", null);
        }

        if (request.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return (false, "Không thể tạo tour với start_date trong quá khứ.", null);
        }

        if (request.PreferredGuideId.HasValue)
        {
            var preferredGuideExists = await _tourRepository.ExistsGuideAsync(request.PreferredGuideId.Value);
            if (!preferredGuideExists)
            {
                return (false, "Không tìm thấy hướng dẫn viên được chọn.", null);
            }
        }

        var durationDays = (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1;

        var tour = new Tour
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            LocationId = request.LocationId,
            GuideId = request.PreferredGuideId,
            PricePerPerson = request.BudgetPerPerson,
            MaxPeople = request.PeopleCount,
            DurationDays = durationDays,
            Rating = 0,
            IsActive = true,
            IsCustomRequest = true,
            RequestedByUserId = actorId,
            GuideRequestStatus = request.PreferredGuideId.HasValue ? TourGuideRequestStatus.Pending : TourGuideRequestStatus.Rejected,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tourRepository.AddTourAsync(tour);

        await _tourRepository.AddTourScheduleAsync(new TourSchedule
        {
            TourId = tour.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AvailableSlots = request.PeopleCount,
            CreatedAt = DateTime.UtcNow
        });

        var createdTour = await _tourRepository.GetTourForOperationsAsync(tour.Id);

        return (true,
            request.PreferredGuideId.HasValue
                ? "Tạo yêu cầu tour thành công. Đang chờ hướng dẫn viên xác nhận."
                : "Tạo yêu cầu tour thành công. Vui lòng chọn hướng dẫn viên phù hợp.",
            createdTour is null ? null : MapToDto(createdTour));
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
            IsCustomRequest = false,
            RequestedByUserId = null,
            GuideRequestStatus = TourGuideRequestStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tourRepository.AddTourAsync(tour);

        var createdTour = await _tourRepository.GetTourByIdAsync(tour.Id);
        var response = createdTour is null ? null : MapToDto(createdTour);

        return (true, "Tạo tour thành công.", response);
    }

    public async Task<(bool Success, string Message)> AssignGuideToRequestedTourAsync(Guid tourId, Guid actorId, Guid guideId)
    {
        var tour = await _tourRepository.GetTourForOperationsAsync(tourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        if (!tour.IsCustomRequest)
        {
            return (false, "Tour này không phải yêu cầu từ người dùng.");
        }

        var isRequestOwner = await _tourRepository.IsTourRequestedByUserAsync(tourId, actorId);
        if (!isRequestOwner)
        {
            return (false, "Bạn không có quyền chọn hướng dẫn viên cho tour này.");
        }

        var guideExists = await _tourRepository.ExistsGuideAsync(guideId);
        if (!guideExists)
        {
            return (false, "Không tìm thấy hướng dẫn viên.");
        }

        tour.GuideId = guideId;
        tour.GuideRequestStatus = TourGuideRequestStatus.Pending;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateTourAsync(tour);

        return (true, "Đã gửi yêu cầu đến hướng dẫn viên thành công.");
    }

    public async Task<(bool Success, string Message)> RespondToRequestedTourAsync(Guid tourId, Guid actorId, bool accept)
    {
        var tour = await _tourRepository.GetTourForOperationsAsync(tourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        if (!tour.IsCustomRequest)
        {
            return (false, "Tour này không phải yêu cầu từ người dùng.");
        }

        var guideId = await _tourRepository.GetGuideIdByUserIdAsync(actorId);
        if (!guideId.HasValue)
        {
            return (false, "Bạn không phải hướng dẫn viên.");
        }

        var isAssigned = await _tourRepository.IsGuideAssignedToTourAsync(tourId, guideId.Value);
        if (!isAssigned)
        {
            return (false, "Bạn không được gán cho tour này.");
        }

        tour.GuideRequestStatus = accept
            ? TourGuideRequestStatus.Accepted
            : TourGuideRequestStatus.Rejected;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateTourAsync(tour);

        return (true, accept ? "Bạn đã nhận tour thành công." : "Bạn đã từ chối tour.");
    }

    public async Task<(bool Success, string Message)> UpdateTourAsync(Guid id, UpdateTourRequestDto request, Guid actorId, bool isAdmin)
    {
        var tour = await _tourRepository.GetTourForOperationsAsync(id);
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

    public async Task<(bool Success, string Message)> CreateScheduleAsync(CreateTourScheduleRequestDto request, Guid actorId, bool isAdmin)
    {
        if (request.EndDate < request.StartDate)
        {
            return (false, "end_date phải lớn hơn hoặc bằng start_date.");
        }

        var tour = await _tourRepository.GetTourForOperationsAsync(request.TourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        await _tourRepository.AddTourScheduleAsync(new TourSchedule
        {
            TourId = request.TourId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AvailableSlots = request.AvailableSlots,
            CreatedAt = DateTime.UtcNow
        });

        return (true, "Tạo lịch tour thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateScheduleAsync(Guid tourId, Guid scheduleId, UpdateTourScheduleRequestDto request, Guid actorId, bool isAdmin)
    {
        if (request.EndDate < request.StartDate)
        {
            return (false, "end_date phải lớn hơn hoặc bằng start_date.");
        }

        var tour = await _tourRepository.GetTourForOperationsAsync(tourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        var schedule = await _tourRepository.GetScheduleByTourIdAsync(tourId, scheduleId);
        if (schedule is null)
        {
            return (false, "Không tìm thấy lịch tour.");
        }

        schedule.StartDate = request.StartDate;
        schedule.EndDate = request.EndDate;
        schedule.AvailableSlots = request.AvailableSlots;

        await _tourRepository.UpdateTourScheduleAsync(schedule);

        return (true, "Cập nhật lịch tour thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteTourAsync(Guid id, Guid actorId, bool isAdmin)
    {
        var tour = await _tourRepository.GetTourForOperationsAsync(id);
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
        var tour = await _tourRepository.GetTourForOperationsAsync(tourId);
        if (tour is null)
        {
            return (false, "Không tìm thấy tour.");
        }

        var permissionResult = await CheckTourPermissionAsync(tour, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            var isRequestOwner = await _tourRepository.IsTourRequestedByUserAsync(tourId, actorId);
            if (!isRequestOwner)
            {
                return permissionResult;
            }
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
            IsCustomRequest = tour.IsCustomRequest,
            RequestedByUserId = tour.RequestedByUserId,
            GuideRequestStatus = tour.GuideRequestStatus.ToString(),
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