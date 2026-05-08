using MusicSchool.Domain.Common;
using MusicSchool.Domain.Students;

namespace MusicSchool.Domain.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = default);
}
