namespace MusicSchool.Application.Payments;

public sealed record RejectPaymentCommand(Guid PaymentId, string Reason);
