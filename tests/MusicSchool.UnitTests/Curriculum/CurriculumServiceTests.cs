using System.Text;
using FluentAssertions;
using Moq;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Curriculum;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Students;
using MusicSchool.UnitTests;

namespace MusicSchool.UnitTests.Curriculum;

public sealed class CurriculumServiceTests
{
    private readonly Mock<ICurriculumRepository> _curriculumRepository = new();
    private readonly Mock<IStudentRepository> _studentRepository = new();
    private readonly Mock<IStudentCurriculumProgressRepository> _progressRepository = new();
    private readonly Mock<IBlobStorageService> _blobStorageService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();

    public CurriculumServiceTests()
    {
        _clock.Setup(clock => clock.UtcNow).Returns(new DateTimeOffset(2026, 5, 8, 12, 0, 0, TimeSpan.Zero));
        _blobStorageService
            .Setup(storage => storage.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream _, string blobName, string _, CancellationToken _) => blobName);
        _blobStorageService
            .Setup(storage => storage.CreateReadUriAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string blobName, TimeSpan _, CancellationToken _) => new Uri($"https://storage.test/{blobName}"));
    }

    [Fact]
    public async Task CreateNodeAsync_WithValidCommand_AddsNodeAndCommits()
    {
        var tenantId = TenantId.New();
        var service = CreateService(tenantId);
        var command = new CreateCurriculumNodeCommand(
            tenantId.Value,
            Guid.NewGuid(),
            ParentNodeId: null,
            "Piano foundations",
            CurriculumNodeType.Module,
            SortOrder: 1);

        var result = await service.CreateNodeAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Piano foundations");
        _curriculumRepository.Verify(repository => repository.AddAsync(It.IsAny<CurriculumNode>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadResourceAsync_WithPdf_AttachesResourceMetadataAndCommits()
    {
        var node = CurriculumNode.Create(TenantId.New(), InstrumentId.New(), null, "Reading notes", CurriculumNodeType.Resource, 1).Value;
        _curriculumRepository.Setup(repository => repository.GetByIdAsync(node.Id, It.IsAny<CancellationToken>())).ReturnsAsync(node);
        var service = CreateService(node.TenantId);

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("pdf"));
        var result = await service.UploadResourceAsync(new UploadCurriculumResourceCommand(
            node.Id.Value,
            Guid.NewGuid(),
            "reading-notes.pdf",
            "application/pdf",
            content));

        result.IsSuccess.Should().BeTrue();
        result.Value.ResourceFileName.Should().Be("reading-notes.pdf");
        result.Value.ResourceFileType.Should().Be(ResourceFileType.Pdf);
        _blobStorageService.Verify(storage => storage.UploadAsync(content, It.IsAny<string>(), "application/pdf", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadResourceAsync_WithUnsupportedFile_ReturnsFailure()
    {
        var node = CurriculumNode.Create(TenantId.New(), InstrumentId.New(), null, "Reading notes", CurriculumNodeType.Resource, 1).Value;
        _curriculumRepository.Setup(repository => repository.GetByIdAsync(node.Id, It.IsAny<CancellationToken>())).ReturnsAsync(node);
        var service = CreateService(node.TenantId);

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("text"));
        var result = await service.UploadResourceAsync(new UploadCurriculumResourceCommand(
            node.Id.Value,
            Guid.NewGuid(),
            "notes.txt",
            "text/plain",
            content));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Curriculum.ResourceTypeUnsupported");
        _blobStorageService.Verify(storage => storage.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProgressAsync_WhenNoProgressExists_CreatesProgress()
    {
        var tenantId = TenantId.New();
        var node = CurriculumNode.Create(tenantId, InstrumentId.New(), null, "Reading notes", CurriculumNodeType.Exercise, 1).Value;
        var student = Student.Create(tenantId, UserId.New(), "Miguel Student").Value;
        _curriculumRepository.Setup(repository => repository.GetByIdAsync(node.Id, It.IsAny<CancellationToken>())).ReturnsAsync(node);
        _studentRepository.Setup(repository => repository.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);
        var service = CreateService(tenantId);

        var result = await service.UpdateProgressAsync(new UpdateStudentCurriculumProgressCommand(
            student.Id.Value,
            node.Id.Value,
            StudentCurriculumProgressStatus.Completed));

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(StudentCurriculumProgressStatus.Completed);
        result.Value.CompletedOnUtc.Should().Be(_clock.Object.UtcNow);
        _progressRepository.Verify(repository => repository.AddAsync(It.IsAny<StudentCurriculumProgress>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateNodeAsync_WhenTenantDoesNotMatchContext_ReturnsMismatch()
    {
        var service = CreateService(TenantId.New());
        var command = new CreateCurriculumNodeCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ParentNodeId: null,
            "Piano foundations",
            CurriculumNodeType.Module,
            SortOrder: 1);

        var result = await service.CreateNodeAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Mismatch");
        _curriculumRepository.Verify(repository => repository.AddAsync(It.IsAny<CurriculumNode>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private CurriculumService CreateService(TenantId tenantId)
    {
        return new CurriculumService(
            _curriculumRepository.Object,
            _studentRepository.Object,
            _progressRepository.Object,
            _blobStorageService.Object,
            _unitOfWork.Object,
            _clock.Object,
            new TestTenantContext(tenantId));
    }
}
