using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MusicSchool.Domain.Users;

namespace MusicSchool.IntegrationTests.Auth;

public static class TestAuthExtensions
{
    public static WebApplicationFactory<Program> WithTestAuth(this WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });
            });
        });
    }

    public static HttpClient CreateAuthenticatedClient(
        this WebApplicationFactory<Program> factory,
        UserRole role = UserRole.Admin,
        Guid? tenantId = null)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        client.DefaultRequestHeaders.Add("X-Test-Role", role.ToString());
        client.DefaultRequestHeaders.Add("X-Tenant-Id", (tenantId ?? Guid.NewGuid()).ToString());
        return client;
    }
}
