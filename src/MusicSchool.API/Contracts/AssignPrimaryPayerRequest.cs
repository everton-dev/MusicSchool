namespace MusicSchool.API.Contracts;

public sealed record AssignPrimaryPayerRequest(Guid GuardianUserId, Guid StudentId);
