using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using MusicSchool.Application.Abstractions;

namespace MusicSchool.Infrastructure;

public sealed class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
    {
        var storageOptions = options.Value;
        _containerClient = new BlobContainerClient(storageOptions.ConnectionString, storageOptions.ContainerName);
    }

    public async Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken).ConfigureAwait(false);
        await blobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return blobClient.Name;
    }

    public Task<Uri> CreateReadUriAsync(string blobName, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!blobClient.CanGenerateSasUri)
        {
            return Task.FromResult(blobClient.Uri);
        }

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(lifetime));
        return Task.FromResult(sasUri);
    }
}
