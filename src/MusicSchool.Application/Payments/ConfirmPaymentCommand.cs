namespace MusicSchool.Application.Payments;

public sealed record ConfirmPaymentCommand(Guid PaymentId, string? PaymentReference);
