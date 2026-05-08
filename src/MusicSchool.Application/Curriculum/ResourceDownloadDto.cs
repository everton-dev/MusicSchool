namespace MusicSchool.Application.Curriculum;

public sealed record ResourceDownloadDto(Uri Uri, DateTimeOffset ExpiresOnUtc);
