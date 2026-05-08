using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Payments;

public interface IPaymentService
{
    Task<Result<PaymentDto>> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<PaymentDto>>> ListAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> CreateManualPaymentAsync(CreateManualPaymentCommand command, CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> ConfirmPaymentAsync(ConfirmPaymentCommand command, CancellationToken cancellationToken = default);

    Task<Result<PaymentDto>> RejectPaymentAsync(RejectPaymentCommand command, CancellationToken cancellationToken = default);
}
