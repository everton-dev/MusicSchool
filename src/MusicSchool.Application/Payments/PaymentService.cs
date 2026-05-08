using System.Globalization;
using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Users;

namespace MusicSchool.Application.Payments;

public sealed class PaymentService(
    IPaymentRepository paymentRepository,
    IFamilyGroupRepository familyGroupRepository,
    IStudentRepository studentRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IEmailSender emailSender,
    ITenantContext tenantContext) : IPaymentService
{
    public async Task<Result<PaymentDto>> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentDto>.Failure(new Error("Payment.IdRequired", "Payment id is required."));
        }

        var payment = await paymentRepository.GetByIdAsync(new PaymentId(paymentId), cancellationToken).ConfigureAwait(false);
        if (payment is null)
        {
            return Result<PaymentDto>.Failure(new Error("Payment.NotFound", "Payment was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, payment.TenantId);
        return tenantResult.IsFailure
            ? Result<PaymentDto>.Failure(tenantResult.Error)
            : Result<PaymentDto>.Success(payment.ToDto());
    }

    public async Task<Result<PagedResult<PaymentDto>>> ListAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var tenantResult = TenantGuard.GetRequiredTenant(tenantContext);
        if (tenantResult.IsFailure)
        {
            return Result<PagedResult<PaymentDto>>.Failure(tenantResult.Error);
        }

        var guardianUserId = query.GuardianUserId.HasValue ? new UserId(query.GuardianUserId.Value) : (UserId?)null;
        var studentId = query.StudentId.HasValue ? new StudentId(query.StudentId.Value) : (StudentId?)null;
        var pagedPayments = await paymentRepository.ListByTenantAsync(
            tenantResult.Value,
            guardianUserId,
            studentId,
            query.Status,
            query.Skip,
            query.NormalizedPageSize,
            query.NormalizedPageNumber,
            cancellationToken).ConfigureAwait(false);

        return Result<PagedResult<PaymentDto>>.Success(new PagedResult<PaymentDto>(
            pagedPayments.Items.Select(payment => payment.ToDto()).ToArray(),
            pagedPayments.PageNumber,
            pagedPayments.PageSize,
            pagedPayments.TotalCount));
    }

    public async Task<Result<PaymentDto>> CreateManualPaymentAsync(CreateManualPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var tenantResult = TenantGuard.EnsureTenant(tenantContext, tenantId);
        if (tenantResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(tenantResult.Error);
        }

        var studentId = new StudentId(command.StudentId);
        var guardianUserId = new UserId(command.GuardianUserId);

        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (student is null || student.TenantId != tenantId)
        {
            return Result<PaymentDto>.Failure(new Error("Student.NotFound", "Student was not found."));
        }

        var guardian = await userRepository.GetByIdAsync(guardianUserId, cancellationToken).ConfigureAwait(false);
        if (guardian is null || guardian.TenantId != tenantId || guardian.Role != UserRole.Guardian)
        {
            return Result<PaymentDto>.Failure(new Error("Guardian.NotFound", "Guardian was not found."));
        }

        var hasPayerRelationship = await familyGroupRepository.HasPrimaryPayerRelationshipAsync(
            tenantId,
            guardianUserId,
            studentId,
            cancellationToken).ConfigureAwait(false);

        if (!hasPayerRelationship)
        {
            return Result<PaymentDto>.Failure(new Error("Payment.GuardianNotPrimaryPayer", "Guardian is not the primary payer for this student."));
        }

        var amountResult = Money.Create(command.Amount, command.Currency);
        if (amountResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(amountResult.Error);
        }

        var paymentResult = Payment.Create(
            tenantId,
            studentId,
            guardianUserId,
            amountResult.Value,
            command.Method,
            command.DueDate,
            command.Description,
            clock.UtcNow);

        if (paymentResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(paymentResult.Error);
        }

        await paymentRepository.AddAsync(paymentResult.Value, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await SendPaymentCreatedEmailAsync(guardian.Email.Value, paymentResult.Value, cancellationToken).ConfigureAwait(false);

        return Result<PaymentDto>.Success(paymentResult.Value.ToDto());
    }

    public async Task<Result<PaymentDto>> ConfirmPaymentAsync(ConfirmPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(new PaymentId(command.PaymentId), cancellationToken).ConfigureAwait(false);
        if (payment is null)
        {
            return Result<PaymentDto>.Failure(new Error("Payment.NotFound", "Payment was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, payment.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(tenantResult.Error);
        }

        var confirmResult = payment.Confirm(command.PaymentReference, clock.UtcNow);
        if (confirmResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(confirmResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var guardian = await userRepository.GetByIdAsync(payment.GuardianUserId, cancellationToken).ConfigureAwait(false);
        if (guardian is not null)
        {
            await SendPaymentConfirmedEmailAsync(guardian.Email.Value, payment, cancellationToken).ConfigureAwait(false);
        }

        return Result<PaymentDto>.Success(payment.ToDto());
    }

    public async Task<Result<PaymentDto>> RejectPaymentAsync(RejectPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(new PaymentId(command.PaymentId), cancellationToken).ConfigureAwait(false);
        if (payment is null)
        {
            return Result<PaymentDto>.Failure(new Error("Payment.NotFound", "Payment was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, payment.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(tenantResult.Error);
        }

        var rejectResult = payment.Reject(command.Reason, clock.UtcNow);
        if (rejectResult.IsFailure)
        {
            return Result<PaymentDto>.Failure(rejectResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<PaymentDto>.Success(payment.ToDto());
    }

    private async Task SendPaymentCreatedEmailAsync(string to, Payment payment, CancellationToken cancellationToken)
    {
        var subject = "New music school payment request";
        var body = string.Create(CultureInfo.InvariantCulture, $"A payment of {payment.Amount.Amount:0.00} {payment.Amount.Currency} is pending for {payment.Description}. Method: {payment.Method}. Due date: {payment.DueDate:yyyy-MM-dd}.");
        await emailSender.SendAsync(to, subject, body, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendPaymentConfirmedEmailAsync(string to, Payment payment, CancellationToken cancellationToken)
    {
        var subject = "Music school payment confirmed";
        var body = string.Create(CultureInfo.InvariantCulture, $"Your payment of {payment.Amount.Amount:0.00} {payment.Amount.Currency} for {payment.Description} has been confirmed.");
        await emailSender.SendAsync(to, subject, body, cancellationToken).ConfigureAwait(false);
    }
}
