using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MusicSchool.API.Contracts;
using MusicSchool.Domain.Users;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests.Auth;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public async Task FamilyGroups_WhenUserIsGuardian_ReturnsForbidden()
    {
        await using var factory = new WebApplicationFactory<Program>().WithTestAuth();
        using var client = factory.CreateAuthenticatedClient(UserRole.Guardian);

        var response = await client.PostAsJsonAsync("/api/family-groups", new CreateFamilyGroupRequest(Guid.NewGuid(), "Silva family"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
