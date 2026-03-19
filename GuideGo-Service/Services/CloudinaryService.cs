using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GuideGo_Service.Dtos.Cloudinary;
using GuideGo_Service.Interfaces;
using Microsoft.Extensions.Options;

namespace GuideGo_Service.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        var cloudinarySettings = settings.Value;

        if (string.IsNullOrWhiteSpace(cloudinarySettings.CloudName) ||
            string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey) ||
            string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing or invalid.");
        }

        var account = new Account(
            cloudinarySettings.CloudName,
            cloudinarySettings.ApiKey,
            cloudinarySettings.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }

    public async Task<(bool Success, string? Url, string Message)> UploadImageAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == Stream.Null || !fileStream.CanRead)
        {
            return (false, null, "Không thể đọc dữ liệu file.");
        }

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "guidego/tours"
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
        {
            return (false, null, $"Upload Cloudinary thất bại: {result.Error.Message}");
        }

        if (string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
        {
            return (false, null, "Upload Cloudinary thất bại: không nhận được URL ảnh.");
        }

        return (true, result.SecureUrl.ToString(), "Upload Cloudinary thành công.");
    }
}
