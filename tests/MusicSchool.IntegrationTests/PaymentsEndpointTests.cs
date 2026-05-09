using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Payments;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;
using MusicSchool.IntegrationTests.Fakes;

namespace MusicSchool.IntegrationTests;

public sealed class PaymentsEndpointTests
{
    [Fact]
    public async Task CreateAndConfirmPayment_CompletesManualPaymentWorkflow()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var createResponse = await client.PostAsJsonAsync("/api/payments", new CreateManualPaymentRequest(
            seedData.TenantId.Value,
            seedData.StudentId.Value,
            seedData.GuardianUserId.Value,
            80.00m,
            "EUR",
            PaymentMethod.BankTransfer,
            new DateOnly(2026, 5, 31),
            "May tuition"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();
        createdPayment.Should().NotBeNull();
        createdPayment!.Status.Should().Be(PaymentStatus.Pending);

        var confirmResponse = await client.PostAsJsonAsync(
            $"/api/payments/{createdPayment.Id}/confirm",
            new ConfirmPaymentRequest("TRF-2026-05-001"));

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmedPayment = await confirmResponse.Content.ReadFromJsonAsync<PaymentDto>();
        confirmedPayment!.Status.Should().Be(PaymentStatus.Confirmed);
        confirmedPayment.PaymentReference.Should().Be("TRF-2026-05-001");

        var listResponse = await client.GetAsync($"/api/payments?status=Confirmed&guardianUserId={seedData.GuardianUserId.Value}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<PaymentSummaryResponse>>();
        page!.TotalCount.Should().Be(1);
        page.Items.Single().Id.Should().Be(createdPayment.Id);
        page.Items.Single().Status.Should().Be("Confirmed");

        var emailSender = factory.Services.GetRequiredService<IEmailSender>().Should().BeOfType<FakeEmailSender>().Subject;
        emailSender.SentEmails.Should().HaveCount(2);
        emailSender.SentEmails.Should().Contain(email => email.To == "guardian@example.com");
    }

    [Fact]
    public async Task CreatePayment_WhenGuardianIsNotPrimaryPayer_ReturnsBadRequest()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory, isPrimaryPayer: false);
        using var client = factory.CreateAuthenticatedClient(UserRole.Admin, seedData.TenantId.Value);

        var response = await client.PostAsJsonAsync("/api/payments", new CreateManualPaymentRequest(
            seedData.TenantId.Value,
            seedData.StudentId.Value,
            seedData.GuardianUserId.Value,
            80.00m,
            "EUR",
            PaymentMethod.MbWay,
            new DateOnly(2026, 5, 31),
            "May tuition"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error!.Code.Should().Be("Payment.GuardianNotPrimaryPayer");
    }

    private static async Task<SeedData> SeedAsync(WebApplicationFactory<Program> factory, bool isPrimaryPayer = true)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicSchoolDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var tenantId = TenantId.New();
        var guardian = User.Create(tenantId, "guardian@example.com", "Guardian", UserRole.Guardian, "en-US", DateTimeOffset.UtcNow).Value;
        var studentUser = User.Create(tenantId, "student@example.com", "Student", UserRole.Student, "en-US", DateTimeOffset.UtcNow).Value;
        var student = Student.Create(tenantId, studentUser.Id, "Miguel Student").Value;
        var familyGroup = FamilyGroup.Create(tenantId, "Silva family", DateTimeOffset.UtcNow).Value;
        familyGroup.AddRelationship(guardian.Id, student.Id, FamilyRelationshipKind.Parent, isPrimaryPayer);

        await dbContext.Users.AddRangeAsync(guardian, studentUser);
        await dbContext.Students.AddAsync(student);
        await dbContext.FamilyGroups.AddAsync(familyGroup);
        await dbContext.SaveChangesAsync();

        return new SeedData(tenantId, student.Id, guardian.Id);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolPaymentsTests-{Guid.NewGuid()}";

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
                    services.RemoveAll<IEmailSender>();
                    services.AddSingleton<IEmailSender, FakeEmailSender>();
                    services.AddDbContext<MusicSchoolDbContext>(options => options.UseInMemoryDatabase(databaseName));
                });
            })
            .WithTestAuth();
    }

    private sealed record SeedData(TenantId TenantId, StudentId StudentId, UserId GuardianUserId);
}
