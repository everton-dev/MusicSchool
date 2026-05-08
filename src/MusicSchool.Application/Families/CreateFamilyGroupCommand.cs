namespace MusicSchool.Application.Families;

public sealed record CreateFamilyGroupCommand(Guid TenantId, string DisplayName);
