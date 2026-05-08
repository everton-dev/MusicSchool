using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Users;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class UserRepository(MusicSchoolDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
