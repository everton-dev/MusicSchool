using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests.Auth;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public async Task FamilyGroupWrites_WhenUserIsGuardian_ReturnForbidden()
    {
        await using var factory = new WebApplicationFactory<Program>().WithTestAuth();
        using var client = factory.CreateAuthenticatedClient(UserRole.Guardian);

        var response = await client.PostAsJsonAsync("/api/family-groups", new CreateFamilyGroupRequest(Guid.NewGuid(), "Silva family"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FamilyGroupList_WhenUserIsGuardian_ReturnsOk()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(UserRole.Guardian);

        var response = await client.GetAsync("/api/family-groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LessonSchedule_WhenUserIsTeacher_ReturnsForbidden()
    {
        await using var factory = new WebApplicationFactory<Program>().WithTestAuth();
        using var client = factory.CreateAuthenticatedClient(UserRole.Teacher);

        var response = await client.PostAsJsonAsync("/api/lessons", new ScheduleIndividualLessonRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(15, 0),
            45,
            "Europe/Lisbon"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolAuthorizationTests-{Guid.NewGuid()}";
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
