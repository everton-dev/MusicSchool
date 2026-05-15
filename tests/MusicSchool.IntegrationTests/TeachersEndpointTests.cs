using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Instruments;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests;

public sealed class TeachersEndpointTests
{
    [Fact]
    public async Task Pause_WhenTeacherExists_MarksTeacherUnavailable()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync($"/api/teachers/{seedData.TeacherId.Value}/pause", new TeacherPauseRequest("Vacation"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        var teacher = await dbContext.Teachers.SingleAsync(item => item.Id == seedData.TeacherId);
        teacher.IsAvailable.Should().BeFalse();
        teacher.AbsenceReason.Should().Be("Vacation");
    }

    [Fact]
    public async Task CreateSchedule_WhenSlotOverlaps_ReturnsConflict()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory, withExistingLesson: true);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync($"/api/teachers/{seedData.TeacherId.Value}/schedule", CreateScheduleRequest(seedData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("SCHEDULE.CONFLICT");
    }

    [Fact]
    public async Task CreateSchedule_WhenValid_CreatesLessonAndPayment()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync($"/api/teachers/{seedData.TeacherId.Value}/schedule", CreateScheduleRequest(seedData));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var schedule = await response.Content.ReadFromJsonAsync<TeacherScheduleResponse>();
        schedule!.BillingUpdated.Should().BeTrue();
        schedule.Lessons.Should().Contain(lesson => lesson.StudentId == seedData.StudentId.Value);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        var paymentExists = await dbContext.Payments.AnyAsync(payment =>
            payment.StudentId == seedData.StudentId &&
            payment.GuardianUserId == seedData.GuardianUserId &&
            payment.Status == PaymentStatus.Pending);
        paymentExists.Should().BeTrue();
    }

    private static CreateTeacherScheduleLessonRequest CreateScheduleRequest(SeedData seedData)
    {
        return new CreateTeacherScheduleLessonRequest(
            seedData.TenantId.Value,
            seedData.StudentId.Value,
            seedData.InstrumentId.Value,
            DayOfWeek.Tuesday,
            new TimeOnly(17, 0),
            60,
            "Weekly");
    }

    private static async Task<SeedData> SeedAsync(WebApplicationFactory<Program> factory, bool withExistingLesson = false)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var tenantId = TenantId.New();
        var instrument = Instrument.Create(tenantId, "Piano").Value;
        var teacherUser = User.Create(tenantId, "teacher@example.test", "Ana Teacher", UserRole.Teacher, "en-US", DateTimeOffset.UtcNow).Value;
        var teacher = Teacher.Create(tenantId, teacherUser.Id, "Ana Teacher").Value;
        teacher.AddInstrument(instrument.Id);
        var guardian = User.Create(tenantId, "guardian@example.test", "Miguel Guardian", UserRole.Guardian, "en-US", DateTimeOffset.UtcNow).Value;
        var studentUser = User.Create(tenantId, "student@example.test", "Sofia Student", UserRole.Student, "en-US", DateTimeOffset.UtcNow).Value;
        var student = Student.Create(tenantId, studentUser.Id, "Sofia Student").Value;
        var familyGroup = FamilyGroup.Create(tenantId, "Guardian household", DateTimeOffset.UtcNow).Value;
        familyGroup.AddRelationship(guardian.Id, student.Id, FamilyRelationshipKind.Guardian, isPrimaryPayer: true);

        await dbContext.Users.AddRangeAsync(teacherUser, guardian, studentUser);
        await dbContext.Instruments.AddAsync(instrument);
        await dbContext.Teachers.AddAsync(teacher);
        await dbContext.Students.AddAsync(student);
        await dbContext.FamilyGroups.AddAsync(familyGroup);

        if (withExistingLesson)
        {
            var schedule = WeeklyLessonSchedule.Create(DayOfWeek.Tuesday, new TimeOnly(16, 30), 60, "Europe/Lisbon").Value;
            var lesson = Lesson.Create(tenantId, teacher.Id, student.Id, instrument.Id, schedule, DateTimeOffset.UtcNow).Value;
            await dbContext.Lessons.AddAsync(lesson);
        }

        await dbContext.SaveChangesAsync();
        return new SeedData(tenantId, teacher.Id, student.Id, instrument.Id, guardian.Id);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolTeachersTests-{Guid.NewGuid()}";
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

    private sealed record SeedData(TenantId TenantId, TeacherId TeacherId, StudentId StudentId, InstrumentId InstrumentId, UserId GuardianUserId);
}
