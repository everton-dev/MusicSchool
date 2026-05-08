using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MusicSchool.API.Auth;

namespace MusicSchool.IntegrationTests.Auth;

public sealed class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenTenantHeaderExists_SetsCurrentTenant()
    {
        var expectedTenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[AuthConstants.TenantHeaderName] = expectedTenantId.ToString();
        var currentTenant = new CurrentTenant();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(httpContext, currentTenant);

        currentTenant.TenantId.Should().NotBeNull();
        currentTenant.TenantId!.Value.Value.Should().Be(expectedTenantId);
    }
}
