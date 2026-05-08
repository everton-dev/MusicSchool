using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Abstractions;

public static class TenantGuard
{
    public static Result<TenantId> GetRequiredTenant(ITenantContext tenantContext)
    {
        return tenantContext.TenantId is null
            ? Result<TenantId>.Failure(new Error("Tenant.Missing", "Authenticated tenant context is required."))
            : Result<TenantId>.Success(tenantContext.TenantId.Value);
    }

    public static Result EnsureTenant(ITenantContext tenantContext, TenantId tenantId)
    {
        if (tenantContext.TenantId is null)
        {
            return Result.Failure(new Error("Tenant.Missing", "Authenticated tenant context is required."));
        }

        if (tenantContext.TenantId.Value != tenantId)
        {
            return Result.Failure(new Error("Tenant.Mismatch", "Requested tenant does not match the authenticated tenant."));
        }

        return Result.Success();
    }
}
