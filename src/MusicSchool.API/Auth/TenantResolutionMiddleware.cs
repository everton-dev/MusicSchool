using System.Security.Claims;
using MusicSchool.Domain.Common;

namespace MusicSchool.API.Auth;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, CurrentTenant currentTenant)
    {
        var tenantValue = ResolveTenantValue(context);
        if (!string.IsNullOrWhiteSpace(tenantValue) && Guid.TryParse(tenantValue, out var tenantId))
        {
            currentTenant.SetTenant(new TenantId(tenantId));
        }

        await next(context).ConfigureAwait(false);
    }

    private static string? ResolveTenantValue(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(AuthConstants.TenantHeaderName, out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        return context.User.FindFirstValue(AuthConstants.TenantClaimType);
    }
}
