using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Domain.Repositories;

public interface IStudentCurriculumProgressRepository
{
    Task<StudentCurriculumProgress?> GetAsync(
        StudentId studentId,
        CurriculumNodeId curriculumNodeId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StudentCurriculumProgress>> ListByStudentAsync(
        TenantId tenantId,
        StudentId studentId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(StudentCurriculumProgress progress, CancellationToken cancellationToken = default);
}
