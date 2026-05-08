using MusicSchool.Domain.Payments;

namespace MusicSchool.API.Contracts;

public sealed record CreateManualPaymentRequest(
    Guid TenantId,
    Guid StudentId,
    Guid GuardianUserId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    DateOnly DueDate,
    string Description);
