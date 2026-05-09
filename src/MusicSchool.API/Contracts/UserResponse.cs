namespace MusicSchool.API.Contracts;

public sealed record UserResponse(
    Guid Id,
    string Name,
    string Profile,
    string FullAddress,
    string PostalCode,
    string DocumentNumber,
    string ContactPhone,
    string Email,
    bool IsActive);
