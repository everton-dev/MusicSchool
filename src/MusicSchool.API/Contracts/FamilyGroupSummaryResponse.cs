namespace MusicSchool.API.Contracts;

public sealed record FamilyGroupSummaryResponse(
    Guid Id,
    string DisplayName,
    int GuardianCount,
    int StudentCount);
