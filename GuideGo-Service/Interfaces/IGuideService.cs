using GuideGo_Service.Dtos.Guide;

namespace GuideGo_Service.Interfaces
{
    public interface IGuideService
    {
        Task<IEnumerable<GuideDto>> GetAllGuides();
        Task<GuideDto?> GetGuideById(Guid id);
        Task<GuideDto?> GetGuideByUserId(Guid userId);
        Task<GuideDto> CreateGuide(CreateGuideDto dto);
        Task<bool> UpdateGuide(Guid id, UpdateGuideDto dto);
        Task<bool> DeleteGuide(Guid id);
        Task<bool> VerifyGuide(Guid id);
        Task<bool> RejectGuide(Guid id);
    }
}
