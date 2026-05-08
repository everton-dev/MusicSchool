namespace MusicSchool.Application.Families;

public sealed record FamilyGroupDto(
    Guid Id,
    Guid TenantId,
    string DisplayName,
    DateTimeOffset CreatedOnUtc,
    IReadOnlyCollection<FamilyRelationshipDto> Relationships);
