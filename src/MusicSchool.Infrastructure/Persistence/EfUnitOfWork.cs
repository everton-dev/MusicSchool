using MusicSchool.Application.Abstractions;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class EfUnitOfWork(MusicSchoolDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
