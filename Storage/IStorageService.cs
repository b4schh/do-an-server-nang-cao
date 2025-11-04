namespace FootballField.API.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string objectName, string contentType, CancellationToken ct = default);
    Task<string> GetPublicUrl(string objectName);
<<<<<<< HEAD
    string GetFullUrl(string relativePath);
=======
>>>>>>> origin/Vu
    Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken ct = default);
    Task DeleteAsync(string objectName, CancellationToken ct = default);
}
