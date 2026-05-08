namespace MusicSchool.API.Contracts;

public sealed record CreateFamilyGroupRequest(Guid TenantId, string DisplayName);
