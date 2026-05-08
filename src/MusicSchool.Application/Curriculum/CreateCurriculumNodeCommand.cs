using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Application.Curriculum;

public sealed record CreateCurriculumNodeCommand(
    Guid TenantId,
    Guid InstrumentId,
    Guid? ParentNodeId,
    string Title,
    CurriculumNodeType Type,
    int SortOrder);
