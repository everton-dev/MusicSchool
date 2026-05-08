using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class FamilyGroupRepository(MusicSchoolDbContext dbContext) : IFamilyGroupRepository
{
    public Task<FamilyGroup?> GetByIdAsync(FamilyGroupId id, CancellationToken cancellationToken = default)
    {
        return dbContext.FamilyGroups
            .Include(familyGroup => familyGroup.Relationships)
            .SingleOrDefaultAsync(familyGroup => familyGroup.Id == id, cancellationToken);
    }

    public async Task<PagedResult<FamilyGroup>> ListByTenantAsync(
        TenantId tenantId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.FamilyGroups
            .Include(familyGroup => familyGroup.Relationships)
            .Where(familyGroup => familyGroup.TenantId == tenantId)
            .OrderBy(familyGroup => familyGroup.DisplayName);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<FamilyGroup>(items, pageNumber, take, totalCount);
    }

    public Task<bool> HasPrimaryPayerRelationshipAsync(
        TenantId tenantId,
        UserId guardianUserId,
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.FamilyRelationships
            .Join(
                dbContext.FamilyGroups,
                relationship => relationship.FamilyGroupId,
                familyGroup => familyGroup.Id,
                (relationship, familyGroup) => new { relationship, familyGroup })
            .AnyAsync(
                item =>
                    item.familyGroup.TenantId == tenantId &&
                    item.relationship.GuardianUserId == guardianUserId &&
                    item.relationship.StudentId == studentId &&
                    item.relationship.IsPrimaryPayer,
                cancellationToken);
    }

    public async Task AddAsync(FamilyGroup familyGroup, CancellationToken cancellationToken = default)
    {
        await dbContext.FamilyGroups.AddAsync(familyGroup, cancellationToken).ConfigureAwait(false);
    }
}
