using MusicSchool.Application.Common;
using MusicSchool.Domain.Lessons;

namespace MusicSchool.Application.Lessons;

public sealed record ListLessonsQuery(
    Guid? TeacherId = null,
    Guid? StudentId = null,
    LessonStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20) : PagedQuery(PageNumber, PageSize);
