using GuideGo_Service.Dtos.Guide;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PRMGuideGo.Controllers
{
    /// <summary>
    /// API quản lý hướng dẫn viên (Guide).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GuidesController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuidesController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        /// <summary>
        /// Lấy danh sách tất cả hướng dẫn viên.
        /// </summary>
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

        /// <summary>
        /// Lấy chi tiết hướng dẫn viên theo id.
        /// </summary>
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

        /// <summary>
        /// Lấy thông tin hướng dẫn viên theo userId.
        /// </summary>
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

        /// <summary>
        /// Đăng ký trở thành hướng dẫn viên, chờ Admin xét duyệt.
        /// </summary>
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

        /// <summary>
        /// Cập nhật thông tin hướng dẫn viên theo id.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateGuideDto dto)
        {
            var result = await _guideService.UpdateGuide(id, dto);

            if (!result)
                return NotFound(new { message = "Guide not found. Update failed." });

            return Ok(new { message = "Guide updated successfully" });
        }

        /// <summary>
        /// Xóa hướng dẫn viên theo id.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _guideService.DeleteGuide(id);

            if (!result)
                return NotFound(new { message = "Guide not found. Delete failed." });

            return Ok(new { message = "Guide deleted successfully" });
        }

        /// <summary>
        /// Duyệt xác minh hướng dẫn viên. Chỉ Admin được phép.
        /// </summary>
        [HttpPut("{id}/verify")]
        public async Task<IActionResult> VerifyGuide(Guid id)
        {
            var result = await _guideService.VerifyGuide(id);

            if (!result)
                return NotFound(new { message = "Guide not found. Verification failed." });

            return Ok(new { message = "Guide verified successfully" });
        }

        /// <summary>
        /// Từ chối yêu cầu đăng ký hướng dẫn viên. Chỉ Admin được phép.
        /// </summary>
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
