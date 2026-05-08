using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;

namespace MusicSchool.UnitTests;

internal sealed class TestTenantContext(TenantId? tenantId) : ITenantContext
{
    public TenantId? TenantId { get; } = tenantId;
}
