using MusicSchool.Domain.Common;
using MusicSchool.Domain.Teachers;

namespace MusicSchool.Domain.Repositories;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = default);
}
