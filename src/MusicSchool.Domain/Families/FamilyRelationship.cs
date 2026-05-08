using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Families;

public sealed class FamilyRelationship : Entity<FamilyRelationshipId>
{
    private FamilyRelationship()
        : base(default)
    {
    }

    internal FamilyRelationship(
        FamilyRelationshipId id,
        FamilyGroupId familyGroupId,
        UserId guardianUserId,
        StudentId studentId,
        FamilyRelationshipKind kind,
        bool isPrimaryPayer)
        : base(id)
    {
        FamilyGroupId = familyGroupId;
        GuardianUserId = guardianUserId;
        StudentId = studentId;
        Kind = kind;
        IsPrimaryPayer = isPrimaryPayer;
    }

    public FamilyGroupId FamilyGroupId { get; private set; }

    public UserId GuardianUserId { get; private set; }

    public StudentId StudentId { get; private set; }

    public FamilyRelationshipKind Kind { get; private set; }

    public bool IsPrimaryPayer { get; private set; }

    internal void MarkAsPrimaryPayer()
    {
        IsPrimaryPayer = true;
    }

    internal void UnmarkAsPrimaryPayer()
    {
        IsPrimaryPayer = false;
    }
}
