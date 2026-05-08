using MusicSchool.Domain.Families;

namespace MusicSchool.Application.Families;

public sealed record FamilyRelationshipDto(
    Guid GuardianUserId,
    Guid StudentId,
    FamilyRelationshipKind Kind,
    bool IsPrimaryPayer);
