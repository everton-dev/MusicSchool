using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Lessons;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Instruments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests;

public sealed class LessonsEndpointTests
{
    [Fact]
    public async Task Schedule_WhenTeacherTeachesInstrument_CreatesLesson()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Teacher, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync("/api/lessons", CreateRequest(seedData));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var lesson = await response.Content.ReadFromJsonAsync<LessonDto>();
        lesson.Should().NotBeNull();
        lesson!.TeacherId.Should().Be(seedData.TeacherId.Value);
        lesson.StudentId.Should().Be(seedData.StudentId.Value);
        lesson.InstrumentId.Should().Be(seedData.InstrumentId.Value);

        var listResponse = await client.GetAsync($"/api/lessons?teacherId={seedData.TeacherId.Value}&pageNumber=1&pageSize=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<LessonDto>>();
        page!.TotalCount.Should().Be(1);
        page.Items.Single().Id.Should().Be(lesson.Id);
    }

    [Fact]
    public async Task Schedule_WhenTeacherAlreadyBooked_ReturnsBadRequest()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Teacher, seedData.TenantId.Value);

        var firstResponse = await client.PostAsJsonAsync("/api/lessons", CreateRequest(seedData));
        var conflictResponse = await client.PostAsJsonAsync("/api/lessons", CreateRequest(seedData, new TimeOnly(17, 30)));

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        conflictResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await conflictResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("Lesson.TeacherScheduleConflict");
    }

    private static ScheduleIndividualLessonRequest CreateRequest(SeedData seedData, TimeOnly? startTime = null)
    {
        return new ScheduleIndividualLessonRequest(
            seedData.TenantId.Value,
            seedData.TeacherId.Value,
            seedData.StudentId.Value,
            seedData.InstrumentId.Value,
            DayOfWeek.Tuesday,
            startTime ?? new TimeOnly(17, 0),
            45,
            "Europe/Lisbon");
    }

    private static async Task<SeedData> SeedAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var tenantId = TenantId.New();
        var instrument = Instrument.Create(tenantId, "Piano").Value;

        var teacher = Teacher.Create(tenantId, UserId.New(), "Ana Teacher").Value;
        teacher.AddInstrument(instrument.Id);

        var student = Student.Create(tenantId, UserId.New(), "Miguel Student").Value;

        await dbContext.Instruments.AddAsync(instrument);
        await dbContext.Teachers.AddAsync(teacher);
        await dbContext.Students.AddAsync(student);
        await dbContext.SaveChangesAsync();

        return new SeedData(tenantId, teacher.Id, student.Id, instrument.Id);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolLessonsTests-{Guid.NewGuid()}";

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

    private sealed record SeedData(TenantId TenantId, TeacherId TeacherId, StudentId StudentId, InstrumentId InstrumentId);
}
