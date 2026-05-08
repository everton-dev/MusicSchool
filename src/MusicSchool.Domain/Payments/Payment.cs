using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Payments;

public sealed class Payment : Entity<PaymentId>
{
    private Payment()
        : base(default)
    {
        Amount = null!;
        Description = string.Empty;
    }

    private Payment(
        PaymentId id,
        TenantId tenantId,
        StudentId studentId,
        UserId guardianUserId,
        Money amount,
        PaymentMethod method,
        DateOnly dueDate,
        string description,
        DateTimeOffset createdOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        StudentId = studentId;
        GuardianUserId = guardianUserId;
        Amount = amount;
        Method = method;
        DueDate = dueDate;
        Description = description;
        Status = PaymentStatus.Pending;
        CreatedOnUtc = createdOnUtc;
    }

    public TenantId TenantId { get; private set; }

    public StudentId StudentId { get; private set; }

    public UserId GuardianUserId { get; private set; }

    public Money Amount { get; private set; }

    public PaymentMethod Method { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateOnly DueDate { get; private set; }

    public string Description { get; private set; }

    public string? PaymentReference { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public DateTimeOffset? ConfirmedOnUtc { get; private set; }

    public DateTimeOffset? RejectedOnUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTimeOffset? CancelledOnUtc { get; private set; }

    public static Result<Payment> Create(
        TenantId tenantId,
        StudentId studentId,
        UserId guardianUserId,
        Money amount,
        PaymentMethod method,
        DateOnly dueDate,
        string description,
        DateTimeOffset createdOnUtc)
    {
        if (tenantId.Value == Guid.Empty || studentId.Value == Guid.Empty || guardianUserId.Value == Guid.Empty)
        {
            return Result<Payment>.Failure(new Error("Payment.IdentityRequired", "Tenant, student, and guardian identifiers are required."));
        }

        if (createdOnUtc.Offset != TimeSpan.Zero)
        {
            return Result<Payment>.Failure(new Error("Time.NotUtc", "Payment creation timestamp must be in UTC."));
        }

        if (string.IsNullOrWhiteSpace(description) || description.Length > 300)
        {
            return Result<Payment>.Failure(new Error("Payment.DescriptionInvalid", "Payment description is required and must not exceed 300 characters."));
        }

        return Result<Payment>.Success(new Payment(
            PaymentId.New(),
            tenantId,
            studentId,
            guardianUserId,
            amount,
            method,
            dueDate,
            description.Trim(),
            createdOnUtc));
    }

    public Result Confirm(string? paymentReference, DateTimeOffset confirmedOnUtc)
    {
        if (Status is PaymentStatus.Confirmed)
        {
            return Result.Failure(new Error("Payment.AlreadyConfirmed", "Payment is already confirmed."));
        }

        if (Status is PaymentStatus.Cancelled)
        {
            return Result.Failure(new Error("Payment.Cancelled", "A cancelled payment cannot be confirmed."));
        }

        if (confirmedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Payment confirmation timestamp must be in UTC."));
        }

        if (!string.IsNullOrWhiteSpace(paymentReference) && paymentReference.Length > 100)
        {
            return Result.Failure(new Error("Payment.ReferenceTooLong", "Payment reference must not exceed 100 characters."));
        }

        Status = PaymentStatus.Confirmed;
        PaymentReference = string.IsNullOrWhiteSpace(paymentReference) ? null : paymentReference.Trim();
        ConfirmedOnUtc = confirmedOnUtc;
        RejectedOnUtc = null;
        RejectionReason = null;

        return Result.Success();
    }

    public Result Reject(string reason, DateTimeOffset rejectedOnUtc)
    {
        if (Status is PaymentStatus.Confirmed)
        {
            return Result.Failure(new Error("Payment.Confirmed", "A confirmed payment cannot be rejected."));
        }

        if (Status is PaymentStatus.Cancelled)
        {
            return Result.Failure(new Error("Payment.Cancelled", "A cancelled payment cannot be rejected."));
        }

        if (rejectedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Payment rejection timestamp must be in UTC."));
        }

        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
        {
            return Result.Failure(new Error("Payment.RejectionReasonInvalid", "Rejection reason is required and must not exceed 500 characters."));
        }

        Status = PaymentStatus.Rejected;
        RejectionReason = reason.Trim();
        RejectedOnUtc = rejectedOnUtc;

        return Result.Success();
    }
}
