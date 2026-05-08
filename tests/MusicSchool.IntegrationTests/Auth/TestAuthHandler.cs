using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicSchool.API.Auth;
using MusicSchool.Domain.Users;

namespace MusicSchool.IntegrationTests.Auth;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue("X-Test-Role", out var roleHeader)
            ? roleHeader.FirstOrDefault()
            : UserRole.Admin.ToString();

        var tenantId = Request.Headers.TryGetValue(AuthConstants.TenantHeaderName, out var tenantHeader)
            ? tenantHeader.FirstOrDefault()
            : Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "integration-test-user"),
            new Claim(ClaimTypes.Role, role ?? UserRole.Admin.ToString()),
            new Claim(AuthConstants.TenantClaimType, tenantId ?? Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
