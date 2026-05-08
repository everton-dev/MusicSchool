using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Application.Families;

public sealed class FamilyGroupService(
    IFamilyGroupRepository familyGroupRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ITenantContext tenantContext) : IFamilyGroupService
{
    public async Task<Result<FamilyGroupDto>> GetByIdAsync(Guid familyGroupId, CancellationToken cancellationToken = default)
    {
        if (familyGroupId == Guid.Empty)
        {
            return Result<FamilyGroupDto>.Failure(new Error("FamilyGroup.IdRequired", "Family group id is required."));
        }

        var familyGroup = await familyGroupRepository.GetByIdAsync(new FamilyGroupId(familyGroupId), cancellationToken).ConfigureAwait(false);
        if (familyGroup is null)
        {
            return Result<FamilyGroupDto>.Failure(new Error("FamilyGroup.NotFound", "Family group was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, familyGroup.TenantId);
        return tenantResult.IsFailure
            ? Result<FamilyGroupDto>.Failure(tenantResult.Error)
            : Result<FamilyGroupDto>.Success(familyGroup.ToDto());
    }

    public async Task<Result<PagedResult<FamilyGroupDto>>> ListAsync(ListFamilyGroupsQuery query, CancellationToken cancellationToken = default)
    {
        var tenantResult = TenantGuard.GetRequiredTenant(tenantContext);
        if (tenantResult.IsFailure)
        {
            return Result<PagedResult<FamilyGroupDto>>.Failure(tenantResult.Error);
        }

        var pagedGroups = await familyGroupRepository.ListByTenantAsync(
            tenantResult.Value,
            query.Skip,
            query.NormalizedPageSize,
            query.NormalizedPageNumber,
            cancellationToken).ConfigureAwait(false);

        return Result<PagedResult<FamilyGroupDto>>.Success(new PagedResult<FamilyGroupDto>(
            pagedGroups.Items.Select(group => group.ToDto()).ToArray(),
            pagedGroups.PageNumber,
            pagedGroups.PageSize,
            pagedGroups.TotalCount));
    }

    public async Task<Result<FamilyGroupDto>> CreateAsync(CreateFamilyGroupCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var tenantResult = TenantGuard.EnsureTenant(tenantContext, tenantId);
        if (tenantResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(tenantResult.Error);
        }

        var familyGroupResult = FamilyGroup.Create(tenantId, command.DisplayName, clock.UtcNow);
        if (familyGroupResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(familyGroupResult.Error);
        }

        await familyGroupRepository.AddAsync(familyGroupResult.Value, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<FamilyGroupDto>.Success(familyGroupResult.Value.ToDto());
    }

    public async Task<Result<FamilyGroupDto>> AddRelationshipAsync(AddFamilyRelationshipCommand command, CancellationToken cancellationToken = default)
    {
        var familyGroup = await familyGroupRepository.GetByIdAsync(new FamilyGroupId(command.FamilyGroupId), cancellationToken).ConfigureAwait(false);
        if (familyGroup is null)
        {
            return Result<FamilyGroupDto>.Failure(new Error("FamilyGroup.NotFound", "Family group was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, familyGroup.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(tenantResult.Error);
        }

        var relationshipResult = familyGroup.AddRelationship(
            new UserId(command.GuardianUserId),
            new StudentId(command.StudentId),
            command.Kind,
            command.IsPrimaryPayer);

        if (relationshipResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(relationshipResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<FamilyGroupDto>.Success(familyGroup.ToDto());
    }

    public async Task<Result<FamilyGroupDto>> AssignPrimaryPayerAsync(AssignPrimaryPayerCommand command, CancellationToken cancellationToken = default)
    {
        var familyGroup = await familyGroupRepository.GetByIdAsync(new FamilyGroupId(command.FamilyGroupId), cancellationToken).ConfigureAwait(false);
        if (familyGroup is null)
        {
            return Result<FamilyGroupDto>.Failure(new Error("FamilyGroup.NotFound", "Family group was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, familyGroup.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(tenantResult.Error);
        }

        var assignmentResult = familyGroup.AssignPrimaryPayer(new UserId(command.GuardianUserId), new StudentId(command.StudentId));
        if (assignmentResult.IsFailure)
        {
            return Result<FamilyGroupDto>.Failure(assignmentResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<FamilyGroupDto>.Success(familyGroup.ToDto());
    }
}
