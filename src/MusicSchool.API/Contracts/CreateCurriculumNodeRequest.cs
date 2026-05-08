using MusicSchool.Domain.Curriculum;

namespace MusicSchool.API.Contracts;

public sealed record CreateCurriculumNodeRequest(
    Guid TenantId,
    Guid InstrumentId,
    Guid? ParentNodeId,
    string Title,
    CurriculumNodeType Type,
    int SortOrder);
