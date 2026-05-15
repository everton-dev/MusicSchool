using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Families;

public sealed class FamilyGroup : Entity<FamilyGroupId>
{
    private readonly List<FamilyRelationship> _relationships = [];

    private FamilyGroup()
        : base(default)
    {
        DisplayName = string.Empty;
    }

    private FamilyGroup(FamilyGroupId id, TenantId tenantId, string displayName, DateTimeOffset createdOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        DisplayName = displayName;
        CreatedOnUtc = createdOnUtc;
    }

    public TenantId TenantId { get; private set; }

    public string DisplayName { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public IReadOnlyCollection<FamilyRelationship> Relationships => _relationships.AsReadOnly();

    public static Result<FamilyGroup> Create(TenantId tenantId, string displayName, DateTimeOffset createdOnUtc)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result<FamilyGroup>.Failure(new Error("Tenant.Required", "Tenant id is required."));
        }

        if (createdOnUtc.Offset != TimeSpan.Zero)
        {
            return Result<FamilyGroup>.Failure(new Error("Time.NotUtc", "Created timestamp must be in UTC."));
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
        {
            return Result<FamilyGroup>.Failure(new Error("Family.DisplayNameInvalid", "Family display name is required and must not exceed 200 characters."));
        }

        return Result<FamilyGroup>.Success(new FamilyGroup(FamilyGroupId.New(), tenantId, displayName.Trim(), createdOnUtc));
    }

    public Result AddRelationship(
        UserId guardianUserId,
        StudentId studentId,
        FamilyRelationshipKind kind,
        bool isPrimaryPayer)
    {
        if (guardianUserId.Value == Guid.Empty || studentId.Value == Guid.Empty)
        {
            return Result.Failure(new Error("FamilyRelationship.IdentityRequired", "Guardian and student identifiers are required."));
        }

        if (_relationships.Any(relationship =>
                relationship.GuardianUserId == guardianUserId && relationship.StudentId == studentId))
        {
            return Result.Failure(new Error("FamilyRelationship.Duplicate", "Guardian is already connected to this student in the family group."));
        }

        if (isPrimaryPayer)
        {
            ClearPrimaryPayerForStudent(studentId);
        }

        _relationships.Add(new FamilyRelationship(
            FamilyRelationshipId.New(),
            Id,
            guardianUserId,
            studentId,
            kind,
            isPrimaryPayer));

        return Result.Success();
    }

    public Result AssignPrimaryPayer(UserId guardianUserId, StudentId studentId)
    {
        var relationship = _relationships.FirstOrDefault(item =>
            item.GuardianUserId == guardianUserId && item.StudentId == studentId);

        if (relationship is null)
        {
            return Result.Failure(new Error("FamilyRelationship.NotFound", "Guardian is not connected to this student in the family group."));
        }

        ClearPrimaryPayerForStudent(studentId);
        relationship.MarkAsPrimaryPayer();

        return Result.Success();
    }

    public IReadOnlyCollection<StudentId> GetStudentsManagedBy(UserId guardianUserId)
    {
        return _relationships
            .Where(relationship => relationship.GuardianUserId == guardianUserId)
            .Select(relationship => relationship.StudentId)
            .Distinct()
            .ToArray();
    }

    public IReadOnlyCollection<StudentId> RemoveRelationshipsNotIn(UserId guardianUserId, IReadOnlyCollection<StudentId> retainedStudentIds)
    {
        var retained = retainedStudentIds.ToHashSet();
        var removedStudentIds = _relationships
            .Where(relationship => relationship.GuardianUserId == guardianUserId && !retained.Contains(relationship.StudentId))
            .Select(relationship => relationship.StudentId)
            .Distinct()
            .ToArray();

        _relationships.RemoveAll(relationship => relationship.GuardianUserId == guardianUserId && !retained.Contains(relationship.StudentId));
        return removedStudentIds;
    }

    private void ClearPrimaryPayerForStudent(StudentId studentId)
    {
        foreach (var relationship in _relationships.Where(relationship => relationship.StudentId == studentId))
        {
            relationship.UnmarkAsPrimaryPayer();
        }
    }
}
