using MusicSchool.Domain.Payments;

namespace MusicSchool.Application.Payments;

public sealed record PaymentDto(
    Guid Id,
    Guid TenantId,
    Guid StudentId,
    Guid GuardianUserId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    PaymentStatus Status,
    DateOnly DueDate,
    string Description,
    string? PaymentReference,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? ConfirmedOnUtc,
    DateTimeOffset? RejectedOnUtc,
    string? RejectionReason);
