namespace MusicSchool.API.Contracts;

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Profile,
    string FullAddress,
    string PostalCode,
    string DocType,
    string DocumentNumber,
    string ContactPhone,
    string Email,
    DateOnly? BirthDate,
    bool IsActive,
    IReadOnlyCollection<HouseholdMemberResponse> HouseholdMembers,
    IReadOnlyCollection<string> LessonTypes,
    int AutoStudentCreatedCount = 0);

public sealed record HouseholdMemberResponse(
    Guid UserId,
    string Name,
    DateOnly? BirthDate,
    string DocType,
    string DocumentNumber,
    string Email,
    bool IsActive);
