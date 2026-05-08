using MusicSchool.Domain.Families;

namespace MusicSchool.API.Contracts;

public sealed record AddFamilyRelationshipRequest(
    Guid GuardianUserId,
    Guid StudentId,
    FamilyRelationshipKind Kind,
    bool IsPrimaryPayer);
