using GuideGo_Service.Dtos.Location;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers;

/// <summary>
/// API quản lý địa điểm.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// API lấy toàn bộ danh sách địa điểm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var locations = await _locationService.GetAllLocationsAsync();
        return Ok(new
        {
            message = "Get all locations successfully",
            data = locations
        });
    }

    /// <summary>
    /// API lấy chi tiết địa điểm theo id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var location = await _locationService.GetLocationByIdAsync(id);
        if (location is null)
        {
            return NotFound(new { message = "Location not found" });
        }

        return Ok(new
        {
            message = "Get location successfully",
            data = location
        });
    }

    /// <summary>
    /// API tạo địa điểm mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            return BadRequest(new { message = errorMessage });
        }

        var location = await _locationService.CreateLocationAsync(dto);
        return Ok(new
        {
            message = "Create location successfully",
            data = location
        });
    }

    /// <summary>
    /// API cập nhật địa điểm theo id.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationDto dto)
    {
        var result = await _locationService.UpdateLocationAsync(id, dto);
        if (!result)
        {
            return NotFound(new { message = "Location not found. Update failed." });
        }

        return Ok(new { message = "Location updated successfully" });
    }

    /// <summary>
    /// API xóa địa điểm theo id.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.DeleteLocationAsync(id);
        if (!result)
        {
            return NotFound(new { message = "Location not found. Delete failed." });
        }

        return Ok(new { message = "Location deleted successfully" });
    }
}