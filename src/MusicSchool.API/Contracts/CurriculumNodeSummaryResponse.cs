namespace MusicSchool.API.Contracts;

public sealed record CurriculumNodeSummaryResponse(
    Guid Id,
    Guid InstrumentId,
    Guid? ParentNodeId,
    string Title,
    string Type,
    int SortOrder,
    bool HasResource);
