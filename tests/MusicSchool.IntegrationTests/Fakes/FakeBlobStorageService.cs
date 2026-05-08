using MusicSchool.Application.Abstractions;

namespace MusicSchool.IntegrationTests.Fakes;

public sealed class FakeBlobStorageService : IBlobStorageService
{
    public List<string> UploadedBlobNames { get; } = [];

    public Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default)
    {
        UploadedBlobNames.Add(blobName);
        return Task.FromResult(blobName);
    }

    public Task<Uri> CreateReadUriAsync(string blobName, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Uri($"https://storage.test/{blobName}"));
    }
}
