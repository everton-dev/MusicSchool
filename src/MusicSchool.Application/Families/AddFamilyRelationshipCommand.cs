using MusicSchool.Domain.Families;

namespace MusicSchool.Application.Families;

public sealed record AddFamilyRelationshipCommand(
    Guid FamilyGroupId,
    Guid GuardianUserId,
    Guid StudentId,
    FamilyRelationshipKind Kind,
    bool IsPrimaryPayer);
