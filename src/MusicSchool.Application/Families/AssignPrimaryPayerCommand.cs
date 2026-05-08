namespace MusicSchool.Application.Families;

public sealed record AssignPrimaryPayerCommand(Guid FamilyGroupId, Guid GuardianUserId, Guid StudentId);
