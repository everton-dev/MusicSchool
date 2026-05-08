using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Application.Curriculum;

public sealed record StudentCurriculumProgressDto(
    Guid Id,
    Guid TenantId,
    Guid StudentId,
    Guid CurriculumNodeId,
    StudentCurriculumProgressStatus Status,
    DateTimeOffset UpdatedOnUtc,
    DateTimeOffset? CompletedOnUtc);
