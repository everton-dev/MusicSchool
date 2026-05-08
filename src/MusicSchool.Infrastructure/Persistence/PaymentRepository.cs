using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class PaymentRepository(MusicSchoolDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Payments.SingleOrDefaultAsync(payment => payment.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Payment>> ListByTenantAsync(
        TenantId tenantId,
        UserId? guardianUserId,
        StudentId? studentId,
        PaymentStatus? status,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Payments.Where(payment => payment.TenantId == tenantId);

        if (guardianUserId.HasValue)
        {
            query = query.Where(payment => payment.GuardianUserId == guardianUserId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(payment => payment.StudentId == studentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(payment => payment.Status == status.Value);
        }

        query = query.OrderByDescending(payment => payment.DueDate).ThenBy(payment => payment.Description);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<Payment>(items, pageNumber, take, totalCount);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken).ConfigureAwait(false);
    }
}
