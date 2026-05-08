using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class StudentCurriculumProgressRepository(MusicSchoolDbContext dbContext) : IStudentCurriculumProgressRepository
{
    public Task<StudentCurriculumProgress?> GetAsync(
        StudentId studentId,
        CurriculumNodeId curriculumNodeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.StudentCurriculumProgress.SingleOrDefaultAsync(
            progress => progress.StudentId == studentId && progress.CurriculumNodeId == curriculumNodeId,
            cancellationToken);
    }

    public async Task<PagedResult<StudentCurriculumProgress>> ListByStudentAsync(
        TenantId tenantId,
        StudentId studentId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.StudentCurriculumProgress
            .Where(progress => progress.TenantId == tenantId && progress.StudentId == studentId)
            .OrderByDescending(progress => progress.UpdatedOnUtc);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<StudentCurriculumProgress>(items, pageNumber, take, totalCount);
    }

    public async Task AddAsync(StudentCurriculumProgress progress, CancellationToken cancellationToken = default)
    {
        await dbContext.StudentCurriculumProgress.AddAsync(progress, cancellationToken).ConfigureAwait(false);
    }
}
