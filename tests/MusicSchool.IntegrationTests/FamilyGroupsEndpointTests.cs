using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Families;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests;

public sealed class FamilyGroupsEndpointTests
{
    [Fact]
    public async Task CreateAndAddRelationship_ReturnsFamilyGroupWithRelationship()
    {
        await using var factory = CreateFactory();
        var tenantId = Guid.NewGuid();
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, tenantId);
        var guardianUserId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync("/api/family-groups", new CreateFamilyGroupRequest(tenantId, "Silva family"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdFamilyGroup = await createResponse.Content.ReadFromJsonAsync<FamilyGroupDto>();
        createdFamilyGroup.Should().NotBeNull();

        var relationshipResponse = await client.PostAsJsonAsync(
            $"/api/family-groups/{createdFamilyGroup!.Id}/relationships",
            new AddFamilyRelationshipRequest(guardianUserId, studentId, FamilyRelationshipKind.Parent, IsPrimaryPayer: true));

        relationshipResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedFamilyGroup = await relationshipResponse.Content.ReadFromJsonAsync<FamilyGroupDto>();

        updatedFamilyGroup.Should().NotBeNull();
        updatedFamilyGroup!.Relationships.Should().ContainSingle();
        updatedFamilyGroup.Relationships.Single().GuardianUserId.Should().Be(guardianUserId);
        updatedFamilyGroup.Relationships.Single().StudentId.Should().Be(studentId);
        updatedFamilyGroup.Relationships.Single().IsPrimaryPayer.Should().BeTrue();

        var listResponse = await client.GetAsync("/api/family-groups?pageNumber=1&pageSize=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<FamilyGroupDto>>();
        page!.TotalCount.Should().Be(1);
        page.Items.Single().Id.Should().Be(createdFamilyGroup.Id);

        using var otherTenantClient = factory.CreateAuthenticatedClient(UserRole.Admin, Guid.NewGuid());
        var otherTenantListResponse = await otherTenantClient.GetAsync("/api/family-groups?pageNumber=1&pageSize=10");
        otherTenantListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var otherTenantPage = await otherTenantListResponse.Content.ReadFromJsonAsync<PagedResult<FamilyGroupDto>>();
        otherTenantPage!.TotalCount.Should().Be(0);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolTests-{Guid.NewGuid()}";
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    var dbContextConfigurationDescriptors = services
                        .Where(descriptor => descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal) == true)
                        .ToArray();

                    foreach (var descriptor in dbContextConfigurationDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.RemoveAll<MusicSchoolDbContext>();
                    services.RemoveAll<DbContextOptions>();
                    services.RemoveAll<DbContextOptions<MusicSchoolDbContext>>();
                    services.RemoveAll<Microsoft.EntityFrameworkCore.Storage.IDatabaseProvider>();
                    services.AddDbContext<MusicSchoolDbContext>(options => options.UseInMemoryDatabase(databaseName));
                });
            })
            .WithTestAuth();
    }
}
