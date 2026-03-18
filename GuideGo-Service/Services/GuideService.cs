using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.Guide;
using GuideGo_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GuideGo_Service.Services
{
    public class GuideService : IGuideService
    {
        private readonly IGuideRepository _guideRepository;
        private readonly IUserRepository _userRepository;

        public GuideService(IGuideRepository guideRepository, IUserRepository userRepository)
        {
            _guideRepository = guideRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<GuideDto>> GetAllGuides()
        {
            var guides = await _guideRepository.GetAllAsync();

            return guides.Select(g => new GuideDto
            {
                Id = g.Id,
                UserId = g.UserId,
                ExperienceYears = g.ExperienceYears,
                Languages = g.Languages,
                Description = g.Description,
                Rating = g.Rating,
                IsVerified = g.IsVerified
            });
        }

        public async Task<GuideDto?> GetGuideById(Guid id)
        {
            var guide = await _guideRepository.GetByIdAsync(id);

            if (guide == null) return null;

            return new GuideDto
            {
                Id = guide.Id,
                UserId = guide.UserId,
                ExperienceYears = guide.ExperienceYears,
                Languages = guide.Languages,
                Description = guide.Description,
                Rating = guide.Rating,
                IsVerified = guide.IsVerified
            };
        }

        public async Task<GuideDto?> GetGuideByUserId(Guid userId)
        {
            var guide = await _guideRepository.GetByUserIdAsync(userId);

            if (guide == null) return null;

            return new GuideDto
            {
                Id = guide.Id,
                UserId = guide.UserId,
                ExperienceYears = guide.ExperienceYears,
                Languages = guide.Languages,
                Description = guide.Description,
                Rating = guide.Rating,
                IsVerified = guide.IsVerified
            };
        }

        public async Task<GuideDto> CreateGuide(CreateGuideDto dto)
        {
            var existingGuide = await _guideRepository.GetByUserIdAsync(dto.UserId);

            if (existingGuide != null)
            {
                return null;
            }

            var guide = new Guide
            {
                UserId = dto.UserId,
                ExperienceYears = dto.ExperienceYears,
                Languages = dto.Languages,
                Description = dto.Description,
                Rating = 0,
                IsVerified = false
            };

            await _guideRepository.AddAsync(guide);
            await _guideRepository.SaveChangesAsync();

            return new GuideDto
            {
                Id = guide.Id,
                UserId = guide.UserId,
                ExperienceYears = guide.ExperienceYears,
                Languages = guide.Languages,
                Description = guide.Description,
                Rating = guide.Rating,
                IsVerified = guide.IsVerified
            };
        }

        public async Task<bool> UpdateGuide(Guid id, UpdateGuideDto dto)
        {
            var guide = await _guideRepository.GetByIdAsync(id);

            if (guide == null)
                return false;

            if (dto.ExperienceYears.HasValue)
                guide.ExperienceYears = dto.ExperienceYears.Value;

            if (dto.Languages != null)
                guide.Languages = dto.Languages;

            if (dto.Description != null)
                guide.Description = dto.Description;

            _guideRepository.Update(guide);
            await _guideRepository.SaveChangesAsync();

            return true;
        }
        [Authorize(Roles = "Admin")]
        public async Task<bool> DeleteGuide(Guid id)
        {
            var guide = await _guideRepository.GetByIdAsync(id);

            if (guide == null)
                return false;

            var user = await _userRepository.GetByIdAsync(guide.UserId);

            if (user != null)
            {
                user.Role = UserRole.Tourist;
                _userRepository.Update(user);
            }

            _guideRepository.Remove(guide);

            await _guideRepository.SaveChangesAsync();

            return true;
        }
        [Authorize(Roles = "Admin")]
        public async Task<bool> VerifyGuide(Guid id)
        {
            var guide = await _guideRepository.GetByIdAsync(id);

            if (guide == null)
                return false;

            guide.IsVerified = true;

            var user = await _userRepository.GetByIdAsync(guide.UserId);

            if (user == null)
                return false;

            user.Role = UserRole.Guide;

            _guideRepository.Update(guide);
            _userRepository.Update(user);

            await _guideRepository.SaveChangesAsync();

            return true;
        }
        [Authorize(Roles = "Admin")]
        public async Task<bool> RejectGuide(Guid id)
        {
            var guide = await _guideRepository.GetByIdAsync(id);

            if (guide == null)
                return false;

            _guideRepository.Remove(guide);
            await _guideRepository.SaveChangesAsync();

            return true;
        }

    }
}
