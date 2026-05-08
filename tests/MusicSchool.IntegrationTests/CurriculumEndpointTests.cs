using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Curriculum;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Instruments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using MusicSchool.IntegrationTests.Auth;
using MusicSchool.IntegrationTests.Fakes;

namespace MusicSchool.IntegrationTests;

public sealed class CurriculumEndpointTests
{
    [Fact]
    public async Task CreateUploadDownloadAndUpdateProgress_CompletesCurriculumWorkflow()
    {
        await using var factory = CreateFactory();
        var seedData = await SeedAsync(factory);
        using var client = factory.CreateAuthenticatedClient(UserRole.Teacher, seedData.TenantId.Value);

        var createResponse = await client.PostAsJsonAsync("/api/curriculum-nodes", new CreateCurriculumNodeRequest(
            seedData.TenantId.Value,
            seedData.InstrumentId.Value,
            ParentNodeId: null,
            "Piano foundations",
            CurriculumNodeType.Resource,
            SortOrder: 1));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var node = await createResponse.Content.ReadFromJsonAsync<CurriculumNodeDto>();
        node.Should().NotBeNull();

        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent(seedData.TeacherId.Value.ToString()), "uploadedByTeacherId");
        multipartContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("fake pdf")), "file", "foundations.pdf");
        multipartContent.Last().Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        var uploadResponse = await client.PostAsync($"/api/curriculum-nodes/{node!.Id}/resources", multipartContent);

        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadedNode = await uploadResponse.Content.ReadFromJsonAsync<CurriculumNodeDto>();
        uploadedNode!.ResourceFileType.Should().Be(ResourceFileType.Pdf);
        uploadedNode.ResourceFileName.Should().Be("foundations.pdf");

        var listNodesResponse = await client.GetAsync($"/api/curriculum-nodes?instrumentId={seedData.InstrumentId.Value}");
        listNodesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var nodesPage = await listNodesResponse.Content.ReadFromJsonAsync<PagedResult<CurriculumNodeDto>>();
        nodesPage!.TotalCount.Should().Be(1);

        var downloadResponse = await client.PostAsync($"/api/curriculum-nodes/{node.Id}/resources/download-url", content: null);

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<ResourceDownloadDto>();
        download!.Uri.Host.Should().Be("storage.test");

        var progressResponse = await client.PutAsJsonAsync(
            $"/api/students/{seedData.StudentId.Value}/curriculum/{node.Id}/progress",
            new UpdateStudentCurriculumProgressRequest(StudentCurriculumProgressStatus.Completed));

        progressResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var progress = await progressResponse.Content.ReadFromJsonAsync<StudentCurriculumProgressDto>();
        progress!.Status.Should().Be(StudentCurriculumProgressStatus.Completed);
        progress.CompletedOnUtc.Should().NotBeNull();

        var listProgressResponse = await client.GetAsync($"/api/students/{seedData.StudentId.Value}/curriculum/progress");
        listProgressResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var progressPage = await listProgressResponse.Content.ReadFromJsonAsync<PagedResult<StudentCurriculumProgressDto>>();
        progressPage!.TotalCount.Should().Be(1);
        progressPage.Items.Single().CurriculumNodeId.Should().Be(node.Id);
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

        return new SeedData(tenantId, instrument.Id, teacher.Id, student.Id);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = $"MusicSchoolCurriculumTests-{Guid.NewGuid()}";

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
                    services.RemoveAll<IBlobStorageService>();
                    services.AddSingleton<IBlobStorageService, FakeBlobStorageService>();
                    services.AddDbContext<MusicSchoolDbContext>(options => options.UseInMemoryDatabase(databaseName));
                });
            })
            .WithTestAuth();
    }

    private sealed record SeedData(TenantId TenantId, InstrumentId InstrumentId, TeacherId TeacherId, StudentId StudentId);
}
