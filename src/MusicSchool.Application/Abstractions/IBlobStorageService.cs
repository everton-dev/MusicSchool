namespace MusicSchool.Application.Abstractions;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default);

    Task<Uri> CreateReadUriAsync(string blobName, TimeSpan lifetime, CancellationToken cancellationToken = default);
}
