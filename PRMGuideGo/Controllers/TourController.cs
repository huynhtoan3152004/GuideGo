using System.Security.Claims;
using GuideGo_Service.Dtos.Tour;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý tour và thông tin chi tiết tour.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly ITourService _tourService;
    private readonly ICloudinaryService _cloudinaryService;

    public TourController(ITourService tourService, ICloudinaryService cloudinaryService)
    {
        _tourService = tourService;
        _cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// Lấy danh sách tất cả tour đang hoạt động, kèm Location, Guide và Schedule.
    /// </summary>
    /// <returns>Danh sách tour đang hoạt động.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllActiveTours()
    {
        var tours = await _tourService.GetAllActiveToursAsync();
        return Ok(tours);
    }

    /// <summary>
    /// Lấy danh sách tour request do chính người dùng tạo.
    /// </summary>
    [HttpGet("my-requests")]
    [Authorize(Roles = "Tourist,Company,Admin")]
    public async Task<IActionResult> GetMyRequests()
    {
        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var tours = await _tourService.GetMyRequestedToursAsync(actorId);
        return Ok(tours);
    }

    /// <summary>
    /// Tìm kiếm tour theo từ khóa, địa điểm, giá, ngày đi, ngôn ngữ và trạng thái xác minh guide.
    /// </summary>
    /// <param name="request">Bộ lọc tìm kiếm tour.</param>
    /// <returns>Kết quả tìm kiếm có phân trang.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchToursRequestDto request)
    {
        var result = await _tourService.SearchToursAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Gợi ý hướng dẫn viên phù hợp theo location/language.
    /// </summary>
    [HttpGet("suitable-guides")]
    [Authorize(Roles = "Tourist,Company,Admin")]
    public async Task<IActionResult> GetSuitableGuides(
        [FromQuery(Name = "location_id")] Guid locationId,
        [FromQuery(Name = "language")] string? language,
        [FromQuery(Name = "verified_only")] bool verifiedOnly = true,
        [FromQuery(Name = "limit")] int limit = 20)
    {
        var guides = await _tourService.GetSuitableGuidesAsync(locationId, language, verifiedOnly, limit);
        return Ok(guides);
    }

    /// <summary>
    /// Lấy chi tiết một tour theo id.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <returns>Thông tin chi tiết tour.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tour = await _tourService.GetTourByIdAsync(id);
        if (tour is null)
        {
            return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = "Không tìm thấy tour." });
        }

        return Ok(tour);
    }

    /// <summary>
    /// Tạo mới tour. Chỉ Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="request">Thông tin tạo tour.</param>
    /// <returns>Kết quả tạo tour.</returns>
    [HttpPost]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.CreateTourAsync(request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Người dùng tạo yêu cầu tour riêng và có thể chọn guide ưu tiên.
    /// </summary>
    [HttpPost("requests")]
    [Authorize(Roles = "Tourist,Company,Admin")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateUserTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var result = await _tourService.CreateUserTourRequestAsync(request, actorId);
        if (!result.Success)
        {
            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = result.Message,
            data = result.Data
        });
    }

    /// <summary>
    /// Người dùng chọn guide cho yêu cầu tour của mình.
    /// </summary>
    [HttpPost("{id:guid}/assign-guide")]
    [Authorize(Roles = "Tourist,Company,Admin")]
    public async Task<IActionResult> AssignGuide(Guid id, [FromBody] AssignGuideToTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var result = await _tourService.AssignGuideToRequestedTourAsync(id, actorId, request.GuideId);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Guide phản hồi có nhận tour request hay không.
    /// </summary>
    [HttpPost("{id:guid}/guide-decision")]
    [Authorize(Roles = "Guide")]
    public async Task<IActionResult> GuideDecision(Guid id, [FromBody] GuideDecisionRequestDto request)
    {
        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var result = await _tourService.RespondToRequestedTourAsync(id, actorId, request.Accept);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("không được gán", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Cập nhật tour theo id. Chỉ owner Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <param name="request">Thông tin cập nhật tour.</param>
    /// <returns>Kết quả cập nhật tour.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTourRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.UpdateTourAsync(id, request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Tạo lịch chạy cho tour.
    /// </summary>
    [HttpPost("schedules")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateTourScheduleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.CreateScheduleAsync(request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Cập nhật lịch tour theo tourId và scheduleId.
    /// </summary>
    [HttpPut("{id:guid}/schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> UpdateSchedule(Guid id, Guid scheduleId, [FromBody] UpdateTourScheduleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Dữ liệu không hợp lệ.";

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = errorMessage });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.UpdateScheduleAsync(id, scheduleId, request, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Xóa mềm tour theo id (đặt IsActive = false). Chỉ owner Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <returns>Kết quả xóa tour.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        var result = await _tourService.DeleteTourAsync(id, actorId, isAdmin);
        if (!result.Success)
        {
            if (result.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = result.Message });
            }

            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = result.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = result.Message });
        }

        return Ok(new { statusCode = StatusCodes.Status200OK, message = result.Message });
    }

    /// <summary>
    /// Upload ảnh tour lên Cloudinary và lưu URL vào hệ thống.
    /// Chỉ owner Guide hoặc Admin được phép.
    /// </summary>
    /// <param name="id">Id tour.</param>
    /// <param name="file">File ảnh upload.</param>
    /// <param name="cancellationToken">Token hủy request.</param>
    /// <returns>Kết quả upload ảnh.</returns>
    [HttpPost("{id:guid}/images")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Guide,Admin")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = "Vui lòng chọn file ảnh hợp lệ." });
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = "Chỉ hỗ trợ upload file ảnh." });
        }

        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized(new { statusCode = StatusCodes.Status401Unauthorized, message = "Token không hợp lệ." });
        }

        var isAdmin = User.IsInRole("Admin") ||
                      string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);

        await using var stream = file.OpenReadStream();
        var uploadResult = await _cloudinaryService.UploadImageAsync(stream, file.FileName, cancellationToken);
        if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.Url))
        {
            return BadRequest(new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = uploadResult.Message
            });
        }

        var saveResult = await _tourService.AddTourImageAsync(id, actorId, isAdmin, uploadResult.Url);
        if (!saveResult.Success)
        {
            if (saveResult.Message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                saveResult.Message.Contains("quyền", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { statusCode = StatusCodes.Status403Forbidden, message = saveResult.Message });
            }

            if (saveResult.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                saveResult.Message.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { statusCode = StatusCodes.Status404NotFound, message = saveResult.Message });
            }

            return BadRequest(new { statusCode = StatusCodes.Status400BadRequest, message = saveResult.Message });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = saveResult.Message,
            data = new
            {
                image_url = uploadResult.Url
            }
        });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue("id")
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");
        return Guid.TryParse(userIdClaim, out userId);
    }
}
