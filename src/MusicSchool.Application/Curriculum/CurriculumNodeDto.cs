using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Application.Curriculum;

public sealed record CurriculumNodeDto(
    Guid Id,
    Guid TenantId,
    Guid InstrumentId,
    Guid? ParentNodeId,
    string Title,
    CurriculumNodeType Type,
    int SortOrder,
    string? ResourceFileName,
    ResourceFileType? ResourceFileType,
    string? ResourceContentType,
    Guid? ResourceUploadedByTeacherId,
    DateTimeOffset? ResourceUploadedOnUtc);
