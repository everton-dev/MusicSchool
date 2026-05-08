using MusicSchool.Domain.Common;
using MusicSchool.Domain.Payments;

namespace MusicSchool.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> ListByTenantAsync(
        TenantId tenantId,
        UserId? guardianUserId,
        StudentId? studentId,
        PaymentStatus? status,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
