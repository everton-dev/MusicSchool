namespace MusicSchool.API.Contracts;

public sealed record PaymentSummaryResponse(
    Guid Id,
    Guid StudentId,
    Guid GuardianUserId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    DateOnly DueDate);
