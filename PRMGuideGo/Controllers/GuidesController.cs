using GuideGo_Service.Dtos.Guide;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuidesController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuidesController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var guides = await _guideService.GetAllGuides();

            return Ok(new
            {
                message = "Get all guides successfully",
                data = guides
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var guide = await _guideService.GetGuideById(id);

            if (guide == null)
                return NotFound(new { message = "Guide not found" });

            return Ok(new
            {
                message = "Get guide successfully",
                data = guide
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var guide = await _guideService.GetGuideByUserId(userId);

            if (guide == null)
                return NotFound(new { message = "Guide not found for this user" });

            return Ok(new
            {
                message = "Get guide by user successfully",
                data = guide
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGuideDto dto)
        {
            var guide = await _guideService.CreateGuide(dto);

            if (guide == null)
                return BadRequest(new { message = "User has already registered as a guide" });

            return Ok(new
            {
                message = "Guide registration submitted successfully. Waiting for admin verification.",
                data = guide
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateGuideDto dto)
        {
            var result = await _guideService.UpdateGuide(id, dto);

            if (!result)
                return NotFound(new { message = "Guide not found. Update failed." });

            return Ok(new { message = "Guide updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _guideService.DeleteGuide(id);

            if (!result)
                return NotFound(new { message = "Guide not found. Delete failed." });

            return Ok(new { message = "Guide deleted successfully" });
        }

        [HttpPut("{id}/verify")]
        public async Task<IActionResult> VerifyGuide(Guid id)
        {
            var result = await _guideService.VerifyGuide(id);

            if (!result)
                return NotFound(new { message = "Guide not found. Verification failed." });

            return Ok(new { message = "Guide verified successfully" });
        }

        [HttpDelete("{id}/reject")]
        public async Task<IActionResult> RejectGuide(Guid id)
        {
            var result = await _guideService.RejectGuide(id);

            if (!result)
                return NotFound(new { message = "Guide not found. Reject failed." });

            return Ok(new { message = "Guide request rejected successfully" });
        }
    }
}
