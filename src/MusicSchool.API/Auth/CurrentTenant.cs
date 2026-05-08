using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;

namespace MusicSchool.API.Auth;

public sealed class CurrentTenant : ITenantContext
{
    public TenantId? TenantId { get; private set; }

    public void SetTenant(TenantId tenantId)
    {
        TenantId = tenantId;
    }
}
