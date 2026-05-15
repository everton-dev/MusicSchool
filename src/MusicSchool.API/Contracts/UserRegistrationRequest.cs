using MusicSchool.Domain.Users;

namespace MusicSchool.API.Contracts;

public sealed record UserRegistrationRequest(
    Guid TenantId,
    string Name,
    UserRole Profile,
    string FullAddress,
    string PostalCode,
    string DocumentNumber,
    string ContactPhone,
    string Email,
    IReadOnlyCollection<Guid>? HouseholdUserIds = null,
    UserScheduleSelectionRequest? ScheduleSelection = null,
    string DocType = "CC",
    DateOnly? BirthDate = null,
    bool IsStudent = false,
    IReadOnlyCollection<HouseholdMemberRequest>? HouseholdMembers = null,
    IReadOnlyCollection<string>? LessonTypes = null);

public sealed record HouseholdMemberRequest(
    Guid? UserId,
    string Name,
    DateOnly? BirthDate,
    string DocType,
    string DocumentNumber,
    string Email);
