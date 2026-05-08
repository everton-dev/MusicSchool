using FluentAssertions;
using Moq;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Families;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Repositories;
using MusicSchool.UnitTests;

namespace MusicSchool.UnitTests.Families;

public sealed class FamilyGroupServiceTests
{
    private readonly Mock<IFamilyGroupRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();

    public FamilyGroupServiceTests()
    {
        _clock.Setup(clock => clock.UtcNow).Returns(new DateTimeOffset(2026, 5, 8, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task CreateAsync_WithValidCommand_AddsFamilyGroupAndCommits()
    {
        var tenantId = TenantId.New();
        var service = CreateService(tenantId);
        var command = new CreateFamilyGroupCommand(tenantId.Value, "Silva family");

        var result = await service.CreateAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().Be("Silva family");
        result.Value.TenantId.Should().Be(command.TenantId);
        _repository.Verify(repository => repository.AddAsync(It.IsAny<FamilyGroup>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRelationshipAsync_WhenFamilyGroupExists_AddsRelationshipAndCommits()
    {
        var familyGroup = FamilyGroup.Create(new TenantId(Guid.NewGuid()), "Silva family", DateTimeOffset.UtcNow).Value;
        _repository
            .Setup(repository => repository.GetByIdAsync(familyGroup.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(familyGroup);

        var service = CreateService(familyGroup.TenantId);
        var command = new AddFamilyRelationshipCommand(
            familyGroup.Id.Value,
            Guid.NewGuid(),
            Guid.NewGuid(),
            FamilyRelationshipKind.Parent,
            IsPrimaryPayer: true);

        var result = await service.AddRelationshipAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Relationships.Should().ContainSingle();
        result.Value.Relationships.Single().IsPrimaryPayer.Should().BeTrue();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRelationshipAsync_WhenFamilyGroupDoesNotExist_ReturnsNotFound()
    {
        var service = CreateService(TenantId.New());
        var command = new AddFamilyRelationshipCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            FamilyRelationshipKind.Parent,
            IsPrimaryPayer: true);

        var result = await service.AddRelationshipAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FamilyGroup.NotFound");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignPrimaryPayerAsync_WhenRelationshipExists_CommitsChange()
    {
        var familyGroup = FamilyGroup.Create(new TenantId(Guid.NewGuid()), "Silva family", DateTimeOffset.UtcNow).Value;
        var studentId = StudentId.New();
        var firstGuardianId = UserId.New();
        var secondGuardianId = UserId.New();
        familyGroup.AddRelationship(firstGuardianId, studentId, FamilyRelationshipKind.Parent, isPrimaryPayer: true);
        familyGroup.AddRelationship(secondGuardianId, studentId, FamilyRelationshipKind.Guardian, isPrimaryPayer: false);

        _repository
            .Setup(repository => repository.GetByIdAsync(familyGroup.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(familyGroup);

        var service = CreateService(familyGroup.TenantId);

        var result = await service.AssignPrimaryPayerAsync(new AssignPrimaryPayerCommand(
            familyGroup.Id.Value,
            secondGuardianId.Value,
            studentId.Value));

        result.IsSuccess.Should().BeTrue();
        result.Value.Relationships.Single(relationship => relationship.GuardianUserId == secondGuardianId.Value).IsPrimaryPayer.Should().BeTrue();
        result.Value.Relationships.Single(relationship => relationship.GuardianUserId == firstGuardianId.Value).IsPrimaryPayer.Should().BeFalse();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTenantDoesNotMatchContext_ReturnsMismatch()
    {
        var service = CreateService(TenantId.New());
        var command = new CreateFamilyGroupCommand(Guid.NewGuid(), "Silva family");

        var result = await service.CreateAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Mismatch");
        _repository.Verify(repository => repository.AddAsync(It.IsAny<FamilyGroup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private FamilyGroupService CreateService(TenantId tenantId)
    {
        return new FamilyGroupService(_repository.Object, _unitOfWork.Object, _clock.Object, new TestTenantContext(tenantId));
    }
}
