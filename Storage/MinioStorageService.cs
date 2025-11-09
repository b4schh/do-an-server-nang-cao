using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using System.Web;

namespace FootballField.API.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioSettings _settings;

    public MinioStorageService(IMinioClient client, IOptions<MinioSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        var exists = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_settings.BucketName), ct);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), ct);
        }
    }

    public async Task<string> UploadAsync(Stream stream, string objectName, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketAsync(ct);

        // Nếu stream không seek được → copy sang MemoryStream để lấy Length
        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;
            stream = ms;
        }

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType), ct);

        // Trả về relative path thay vì full URL
        var encoded = HttpUtility.UrlPathEncode(objectName);
        return $"/{_settings.BucketName}/{encoded}";
    }

    public Task<string> GetPublicUrl(string objectName)
    {
        var scheme = _settings.WithSSL ? "https" : "http";
        // Encode object name phòng dấu cách, unicode
        var encoded = HttpUtility.UrlPathEncode(objectName);
        var url = $"{scheme}://{_settings.Endpoint}/{_settings.BucketName}/{encoded}";
        return Task.FromResult(url);
    }

    public string GetFullUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        // Nếu đã là full URL thì trả về luôn (để tương thích với dữ liệu cũ)
        if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
            return relativePath;

        // Ghép base URL + relative path
        return $"{_settings.BaseUrl.TrimEnd('/')}{relativePath}";
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, TimeSpan expiry, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithExpiry((int)expiry.TotalSeconds);
            
        var url = await _client.PresignedGetObjectAsync(args);
        return url;
    }

    public async Task DeleteAsync(string objectName, CancellationToken ct = default)
    {
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs().WithBucket(_settings.BucketName).WithObject(objectName), ct);
    }
}
