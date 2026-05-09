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
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;

namespace MusicSchool.IntegrationTests;

public sealed class UsersEndpointTests
{
    [Fact]
    public async Task CreateGuardian_WithHouseholdAndSchedule_CreatesUserRelationshipAndLesson()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync("/api/users", new UserRegistrationRequest(
            seedData.TenantId.Value,
            "Miguel Guardian",
            UserRole.Guardian,
            "Avenida Central 44, Lisboa",
            "1050-010",
            "ID-20001",
            "+351 910 000 002",
            "miguel.guardian@example.test",
            [seedData.HouseholdUserId.Value],
            new UserScheduleSelectionRequest(
                seedData.TeacherId.Value,
                seedData.InstrumentId.Value,
                DayOfWeek.Wednesday,
                new TimeOnly(17, 0),
                60,
                "Europe/Lisbon")));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.Should().NotBeNull();
        user!.Profile.Should().Be("Guardian");
        user.IsActive.Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        var guardianId = new UserId(user.Id);
        var guardianStudent = await dbContext.Students.SingleAsync(student => student.UserId == guardianId);
        var familyGroup = await dbContext.FamilyGroups.Include(group => group.Relationships).SingleAsync();
        familyGroup.Relationships.Should().Contain(relationship => relationship.GuardianUserId == guardianId);
        var lessonCreated = await dbContext.Lessons.AnyAsync(lesson =>
            lesson.StudentId == guardianStudent.Id &&
            lesson.TeacherId == seedData.TeacherId &&
            lesson.Schedule.DayOfWeek == DayOfWeek.Wednesday &&
            lesson.Schedule.StartTime == new TimeOnly(17, 0));
        lessonCreated.Should().BeTrue();
    }

    [Fact]
    public async Task GetTeacherScheduleOptions_WhenSlotIsBooked_ReturnsDisabledSlotWithStudentName()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory, withBookedSlot: true);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.GetAsync("/api/users/teacher-schedule-options?instrumentQuery=Piano");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var options = await response.Content.ReadFromJsonAsync<TeacherScheduleOptionResponse[]>();
        options.Should().NotBeNull();
        options.Should().Contain(option =>
            option.TeacherId == seedData.TeacherId.Value &&
            option.InstrumentId == seedData.InstrumentId.Value &&
            option.DayOfWeek == DayOfWeek.Monday &&
            option.StartTime == new TimeOnly(15, 30) &&
            option.IsTaken &&
            option.AssignedStudentName == "Booked Student");
        options.Should().Contain(option => !option.IsTaken);
    }

    private static async Task<SeedData> SeedAsync(WebApplicationFactory<Program> factory, bool withBookedSlot = false)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var tenantId = TenantId.New();
        var instrument = Instrument.Create(tenantId, "Piano").Value;
        var teacherUser = CreateUser(tenantId, "teacher@example.test", "Ana Teacher", UserRole.Teacher);
        var teacher = Teacher.Create(tenantId, teacherUser.Id, "Ana Teacher").Value;
        teacher.AddInstrument(instrument.Id);

        var householdUser = CreateUser(tenantId, "household@example.test", "Household Student", UserRole.Student);
        var householdStudent = Student.Create(tenantId, householdUser.Id, "Household Student").Value;

        await dbContext.Users.AddRangeAsync(teacherUser, householdUser);
        await dbContext.Instruments.AddAsync(instrument);
        await dbContext.Teachers.AddAsync(teacher);
        await dbContext.Students.AddAsync(householdStudent);

        if (withBookedSlot)
        {
            var bookedUser = CreateUser(tenantId, "booked@example.test", "Booked Student", UserRole.Student);
            var bookedStudent = Student.Create(tenantId, bookedUser.Id, "Booked Student").Value;
            var schedule = WeeklyLessonSchedule.Create(DayOfWeek.Monday, new TimeOnly(15, 30), 45, "Europe/Lisbon").Value;
            var lesson = Lesson.Create(tenantId, teacher.Id, bookedStudent.Id, instrument.Id, schedule, DateTimeOffset.UtcNow).Value;
            await dbContext.Users.AddAsync(bookedUser);
            await dbContext.Students.AddAsync(bookedStudent);
            await dbContext.Lessons.AddAsync(lesson);
        }

        await dbContext.SaveChangesAsync();
        return new SeedData(tenantId, teacher.Id, instrument.Id, householdUser.Id);
    }

    private static User CreateUser(TenantId tenantId, string email, string name, UserRole role)
    {
        return User.Create(
            tenantId,
            email,
            name,
            role,
            "en-US",
            DateTimeOffset.UtcNow,
            "Rua da Escola 12, Lisboa",
            "1000-001",
            $"DOC-{Guid.NewGuid():N}",
            "+351 910 000 000").Value;
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolUsersTests-{Guid.NewGuid()}";
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

    private sealed record SeedData(TenantId TenantId, TeacherId TeacherId, InstrumentId InstrumentId, UserId HouseholdUserId);
}
