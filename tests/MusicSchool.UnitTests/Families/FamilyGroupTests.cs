using FluentAssertions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;

namespace MusicSchool.UnitTests.Families;

public sealed class FamilyGroupTests
{
    [Fact]
    public void AddRelationship_AllowsOneGuardianToManageMultipleStudents()
    {
        var familyGroup = FamilyGroup.Create(TenantId.New(), "Silva family", DateTimeOffset.UtcNow).Value;
        var guardianUserId = UserId.New();
        var firstStudentId = StudentId.New();
        var secondStudentId = StudentId.New();

        familyGroup.AddRelationship(guardianUserId, firstStudentId, FamilyRelationshipKind.Parent, isPrimaryPayer: true);
        familyGroup.AddRelationship(guardianUserId, secondStudentId, FamilyRelationshipKind.Parent, isPrimaryPayer: true);

        familyGroup.GetStudentsManagedBy(guardianUserId).Should().BeEquivalentTo([firstStudentId, secondStudentId]);
    }

    [Fact]
    public void AddRelationship_RejectsDuplicateGuardianStudentPair()
    {
        var familyGroup = FamilyGroup.Create(TenantId.New(), "Silva family", DateTimeOffset.UtcNow).Value;
        var guardianUserId = UserId.New();
        var studentId = StudentId.New();

        familyGroup.AddRelationship(guardianUserId, studentId, FamilyRelationshipKind.Parent, isPrimaryPayer: true);
        var duplicateResult = familyGroup.AddRelationship(guardianUserId, studentId, FamilyRelationshipKind.Parent, isPrimaryPayer: false);

        duplicateResult.IsFailure.Should().BeTrue();
        duplicateResult.Error.Code.Should().Be("FamilyRelationship.Duplicate");
    }

    [Fact]
    public void AssignPrimaryPayer_LeavesOnlyOnePrimaryPayerForStudent()
    {
        var familyGroup = FamilyGroup.Create(TenantId.New(), "Silva family", DateTimeOffset.UtcNow).Value;
        var firstGuardianId = UserId.New();
        var secondGuardianId = UserId.New();
        var studentId = StudentId.New();

        familyGroup.AddRelationship(firstGuardianId, studentId, FamilyRelationshipKind.Parent, isPrimaryPayer: true);
        familyGroup.AddRelationship(secondGuardianId, studentId, FamilyRelationshipKind.Guardian, isPrimaryPayer: false);

        familyGroup.AssignPrimaryPayer(secondGuardianId, studentId);

        familyGroup.Relationships.Single(relationship => relationship.GuardianUserId == firstGuardianId).IsPrimaryPayer.Should().BeFalse();
        familyGroup.Relationships.Single(relationship => relationship.GuardianUserId == secondGuardianId).IsPrimaryPayer.Should().BeTrue();
    }
}
