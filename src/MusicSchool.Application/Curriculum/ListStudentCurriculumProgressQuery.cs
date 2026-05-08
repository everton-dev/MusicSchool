using MusicSchool.Application.Common;

namespace MusicSchool.Application.Curriculum;

public sealed record ListStudentCurriculumProgressQuery(Guid StudentId, int PageNumber = 1, int PageSize = 20) : PagedQuery(PageNumber, PageSize);
