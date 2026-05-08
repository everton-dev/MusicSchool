using MusicSchool.Domain.Families;

namespace MusicSchool.Application.Families;

public static class FamilyGroupMapper
{
    public static FamilyGroupDto ToDto(this FamilyGroup familyGroup)
    {
        return new FamilyGroupDto(
            familyGroup.Id.Value,
            familyGroup.TenantId.Value,
            familyGroup.DisplayName,
            familyGroup.CreatedOnUtc,
            familyGroup.Relationships
                .Select(relationship => new FamilyRelationshipDto(
                    relationship.GuardianUserId.Value,
                    relationship.StudentId.Value,
                    relationship.Kind,
                    relationship.IsPrimaryPayer))
                .ToArray());
    }
}
