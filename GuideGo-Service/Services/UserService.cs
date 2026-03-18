using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using GuideGo_Repository.Interfaces;
using GuideGo_Service.Dtos.User;
using GuideGo_Service.Interfaces;

namespace GuideGo_Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task EnsureAdminAccountAsync()
    {
        var adminEmail = "admin@gmail.com";
        var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            return;
        }

        var adminUser = new User
        {
            FullName = "admin",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Phone = "1234567890",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(adminUser);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = request.Phone.Trim();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return (false, "Create user unsuccessfully. Email already exists.");
        }

        var existingPhoneUser = await _userRepository.GetByPhoneAsync(normalizedPhone);
        if (existingPhoneUser is not null)
        {
            return (false, "Create user unsuccessfully. Phone already exists.");
        }

        if (!Enum.TryParse<UserRole>(request.Role?.Trim(), true, out var parsedRole))
        {
            return (false, "Create user unsuccessfully. Role is invalid.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = normalizedPhone,
            AvatarUrl = request.AvatarUrl?.Trim(),
            Role = parsedRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return (true, "Create user successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(Guid id, UpdateUserRequestDto request, Guid actorId, bool isAdmin)
    {
        if (!isAdmin && actorId != id)
        {
            return (false, "You do not have permission to update this account.");
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return (false, "User not found.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedPhone = request.Phone?.Trim();
        var emailOwner = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (emailOwner is not null && emailOwner.Id != id)
        {
            return (false, "Update user unsuccessfully. Email already exists.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedPhone))
        {
            var phoneOwner = await _userRepository.GetByPhoneAsync(normalizedPhone);
            if (phoneOwner is not null && phoneOwner.Id != id)
            {
                return (false, "Update user unsuccessfully. Phone already exists.");
            }
        }

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;
        user.Phone = normalizedPhone;
        user.AvatarUrl = request.AvatarUrl?.Trim();

        var hasRoleUpdate = !string.IsNullOrWhiteSpace(request.Role);
        if (hasRoleUpdate)
        {
            if (!Enum.TryParse<UserRole>(request.Role?.Trim(), true, out var parsedRole))
            {
                return (false, "Update user unsuccessfully. Role is invalid.");
            }

            if (isAdmin)
            {
                if (actorId == id)
                {
                    return (false, "Admin cannot update their own role.");
                }

                if (user.Role == UserRole.Admin)
                {
                    return (false, "Admin cannot update role of an Admin account.");
                }

                if (parsedRole == UserRole.Admin)
                {
                    return (false, "Admin cannot promote another user to Admin role.");
                }

                user.Role = parsedRole;
            }
            else
            {
                if (parsedRole != user.Role)
                {
                    return (false, "You cannot change your own role.");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return (true, "Update user successfully.");
    }

    public async Task<(bool Success, string Message)> SoftDeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return (false, "User not found.");
        }

        user.IsActive = false;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return (true, "Delete user successfully.");
    }

    private static UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
