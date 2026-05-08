using MusicSchool.Domain.Payments;

namespace MusicSchool.Application.Payments;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto(
            payment.Id.Value,
            payment.TenantId.Value,
            payment.StudentId.Value,
            payment.GuardianUserId.Value,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Method,
            payment.Status,
            payment.DueDate,
            payment.Description,
            payment.PaymentReference,
            payment.CreatedOnUtc,
            payment.ConfirmedOnUtc,
            payment.RejectedOnUtc,
            payment.RejectionReason);
    }
}
