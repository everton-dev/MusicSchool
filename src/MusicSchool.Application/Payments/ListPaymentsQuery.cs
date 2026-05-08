using MusicSchool.Application.Common;
using MusicSchool.Domain.Payments;

namespace MusicSchool.Application.Payments;

public sealed record ListPaymentsQuery(
    Guid? GuardianUserId = null,
    Guid? StudentId = null,
    PaymentStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20) : PagedQuery(PageNumber, PageSize);
