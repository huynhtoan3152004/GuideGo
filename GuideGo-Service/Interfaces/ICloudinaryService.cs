namespace GuideGo_Service.Interfaces;

public interface ICloudinaryService
{
    Task<(bool Success, string? Url, string Message)> UploadImageAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
