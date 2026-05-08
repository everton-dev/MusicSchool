using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Students;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class StudentRepository(MusicSchoolDbContext dbContext) : IStudentRepository
{
    public Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Students.SingleOrDefaultAsync(student => student.Id == id, cancellationToken);
    }
}
