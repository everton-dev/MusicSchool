using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Families;

public interface IFamilyGroupService
{
    Task<Result<FamilyGroupDto>> GetByIdAsync(Guid familyGroupId, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<FamilyGroupDto>>> ListAsync(ListFamilyGroupsQuery query, CancellationToken cancellationToken = default);

    Task<Result<FamilyGroupDto>> CreateAsync(CreateFamilyGroupCommand command, CancellationToken cancellationToken = default);

    Task<Result<FamilyGroupDto>> AddRelationshipAsync(AddFamilyRelationshipCommand command, CancellationToken cancellationToken = default);

    Task<Result<FamilyGroupDto>> AssignPrimaryPayerAsync(AssignPrimaryPayerCommand command, CancellationToken cancellationToken = default);
}
