using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;

namespace MusicSchool.Domain.Repositories;

public interface IFamilyGroupRepository
{
    Task<FamilyGroup?> GetByIdAsync(FamilyGroupId id, CancellationToken cancellationToken = default);

    Task<PagedResult<FamilyGroup>> ListByTenantAsync(
        TenantId tenantId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<bool> HasPrimaryPayerRelationshipAsync(
        TenantId tenantId,
        UserId guardianUserId,
        StudentId studentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(FamilyGroup familyGroup, CancellationToken cancellationToken = default);
}
