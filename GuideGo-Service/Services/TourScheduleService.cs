using GuideGo_Repository.Entities;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Tour;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class TourScheduleService : ITourScheduleService
{
    private readonly ITourScheduleRepository _scheduleRepository;

    public TourScheduleService(ITourScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IEnumerable<TourScheduleResponseDto>> GetByTourIdAsync(Guid tourId, bool onlyAvailable = false)
    {
        var schedules = await _scheduleRepository.GetByTourIdAsync(tourId);

        if (onlyAvailable)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            schedules = schedules.Where(s => s.StartDate > today).ToList();
        }

        return schedules.Select(MapToDto);
    }

    public async Task<TourScheduleResponseDto?> GetByIdAsync(Guid id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        return schedule is null ? null : MapToDto(schedule);
    }

    public async Task<(bool Success, string Message, TourScheduleResponseDto? Data)> CreateAsync(
        CreateTourScheduleRequestDto request, Guid actorId, bool isAdmin)
    {
        if (!await _scheduleRepository.ExistsActiveTourAsync(request.TourId))
        {
            return (false, "Không tìm thấy tour.", null);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.StartDate < today)
        {
            return (false, "start_date không được là ngày trong quá khứ.", null);
        }

        if (request.EndDate <= request.StartDate)
        {
            return (false, "end_date phải sau start_date.", null);
        }

        var permissionResult = await CheckTourPermissionAsync(request.TourId, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return (false, permissionResult.Message, null);
        }

        var schedule = new TourSchedule
        {
            TourId = request.TourId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AvailableSlots = request.AvailableSlots,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _scheduleRepository.AddAsync(schedule);

        return (true, "Tạo lịch tour thành công.", MapToDto(created));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(
        Guid id, UpdateTourScheduleRequestDto request, Guid actorId, bool isAdmin)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null)
        {
            return (false, "Không tìm thấy lịch tour.");
        }

        if (request.EndDate <= request.StartDate)
        {
            return (false, "end_date phải sau start_date.");
        }

        var permissionResult = await CheckTourPermissionAsync(schedule.TourId, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        schedule.StartDate = request.StartDate;
        schedule.EndDate = request.EndDate;
        schedule.AvailableSlots = request.AvailableSlots;

        await _scheduleRepository.UpdateAsync(schedule);

        return (true, "Cập nhật lịch tour thành công.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid actorId, bool isAdmin)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule is null)
        {
            return (false, "Không tìm thấy lịch tour.");
        }

        if (await _scheduleRepository.HasActiveBookingsAsync(id))
        {
            return (false, "Không thể xóa lịch tour vì đang có đơn đặt chỗ hoạt động.");
        }

        var permissionResult = await CheckTourPermissionAsync(schedule.TourId, actorId, isAdmin);
        if (!permissionResult.Success)
        {
            return permissionResult;
        }

        await _scheduleRepository.DeleteAsync(schedule);

        return (true, "Xóa lịch tour thành công.");
    }

    private async Task<(bool Success, string Message)> CheckTourPermissionAsync(Guid tourId, Guid actorId, bool isAdmin)
    {
        if (isAdmin)
        {
            return (true, string.Empty);
        }

        var guideId = await _scheduleRepository.GetGuideIdByUserIdAsync(actorId);
        if (!guideId.HasValue)
        {
            return (false, "Bạn không có quyền quản lý lịch tour.");
        }

        var isOwner = await _scheduleRepository.IsTourOwnedByGuideAsync(tourId, guideId.Value);
        if (!isOwner)
        {
            return (false, "Bạn không có quyền quản lý lịch tour này.");
        }

        return (true, string.Empty);
    }

    private static TourScheduleResponseDto MapToDto(TourSchedule schedule) => new()
    {
        Id = schedule.Id,
        StartDate = schedule.StartDate,
        EndDate = schedule.EndDate,
        AvailableSlots = schedule.AvailableSlots
    };
}
