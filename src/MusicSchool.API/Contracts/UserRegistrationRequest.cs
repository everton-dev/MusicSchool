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
    UserScheduleSelectionRequest? ScheduleSelection = null);
