using MusicSchool.Application.Common;

namespace MusicSchool.Application.Curriculum;

public sealed record ListCurriculumNodesQuery(
    Guid? InstrumentId = null,
    Guid? ParentNodeId = null,
    int PageNumber = 1,
    int PageSize = 20) : PagedQuery(PageNumber, PageSize);
