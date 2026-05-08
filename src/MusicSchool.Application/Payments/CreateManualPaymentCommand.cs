using MusicSchool.Domain.Payments;

namespace MusicSchool.Application.Payments;

public sealed record CreateManualPaymentCommand(
    Guid TenantId,
    Guid StudentId,
    Guid GuardianUserId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    DateOnly DueDate,
    string Description);
