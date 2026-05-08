using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Teachers;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class TeacherRepository(MusicSchoolDbContext dbContext) : ITeacherRepository
{
    public Task<Teacher?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Teachers
            .Include(teacher => teacher.Instruments)
            .SingleOrDefaultAsync(teacher => teacher.Id == id, cancellationToken);
    }
}
