using MusicSchool.Domain.Common;
using MusicSchool.Domain.Users;

namespace MusicSchool.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
}
