using FluentAssertions;
using Moq;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Payments;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Users;
using MusicSchool.UnitTests;

namespace MusicSchool.UnitTests.Payments;

public sealed class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IFamilyGroupRepository> _familyGroupRepository = new();
    private readonly Mock<IStudentRepository> _studentRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IEmailSender> _emailSender = new();

    public PaymentServiceTests()
    {
        _clock.Setup(clock => clock.UtcNow).Returns(new DateTimeOffset(2026, 5, 8, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task CreateManualPaymentAsync_WhenGuardianIsPrimaryPayer_CreatesPaymentAndSendsEmail()
    {
        var tenantId = TenantId.New();
        var student = Student.Create(tenantId, UserId.New(), "Miguel Student").Value;
        var guardian = User.Create(tenantId, "guardian@example.com", "Guardian", UserRole.Guardian, "en-US", DateTimeOffset.UtcNow).Value;
        SetupStudentGuardianAndRelationship(student, guardian, hasPrimaryPayerRelationship: true);
        var service = CreateService(tenantId);

        var result = await service.CreateManualPaymentAsync(CreateCommand(tenantId, student.Id, guardian.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentStatus.Pending);
        result.Value.Method.Should().Be(PaymentMethod.MbWay);
        _paymentRepository.Verify(repository => repository.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(sender => sender.SendAsync("guardian@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateManualPaymentAsync_WhenGuardianIsNotPrimaryPayer_ReturnsFailure()
    {
        var tenantId = TenantId.New();
        var student = Student.Create(tenantId, UserId.New(), "Miguel Student").Value;
        var guardian = User.Create(tenantId, "guardian@example.com", "Guardian", UserRole.Guardian, "en-US", DateTimeOffset.UtcNow).Value;
        SetupStudentGuardianAndRelationship(student, guardian, hasPrimaryPayerRelationship: false);
        var service = CreateService(tenantId);

        var result = await service.CreateManualPaymentAsync(CreateCommand(tenantId, student.Id, guardian.Id));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.GuardianNotPrimaryPayer");
        _paymentRepository.Verify(repository => repository.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(sender => sender.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenPaymentIsPending_ConfirmsAndSendsEmail()
    {
        var tenantId = TenantId.New();
        var guardian = User.Create(tenantId, "guardian@example.com", "Guardian", UserRole.Guardian, "en-US", DateTimeOffset.UtcNow).Value;
        var payment = CreatePayment(tenantId, StudentId.New(), guardian.Id);
        _paymentRepository.Setup(repository => repository.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _userRepository.Setup(repository => repository.GetByIdAsync(guardian.Id, It.IsAny<CancellationToken>())).ReturnsAsync(guardian);
        var service = CreateService(tenantId);

        var result = await service.ConfirmPaymentAsync(new ConfirmPaymentCommand(payment.Id.Value, "MBW-123"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(PaymentStatus.Confirmed);
        result.Value.PaymentReference.Should().Be("MBW-123");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(sender => sender.SendAsync("guardian@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupStudentGuardianAndRelationship(Student student, User guardian, bool hasPrimaryPayerRelationship)
    {
        _studentRepository.Setup(repository => repository.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);
        _userRepository.Setup(repository => repository.GetByIdAsync(guardian.Id, It.IsAny<CancellationToken>())).ReturnsAsync(guardian);
        _familyGroupRepository
            .Setup(repository => repository.HasPrimaryPayerRelationshipAsync(student.TenantId, guardian.Id, student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasPrimaryPayerRelationship);
    }

    private static CreateManualPaymentCommand CreateCommand(TenantId tenantId, StudentId studentId, UserId guardianUserId)
    {
        return new CreateManualPaymentCommand(
            tenantId.Value,
            studentId.Value,
            guardianUserId.Value,
            75.00m,
            "EUR",
            PaymentMethod.MbWay,
            new DateOnly(2026, 5, 31),
            "May tuition");
    }

    [Fact]
    public async Task CreateManualPaymentAsync_WhenTenantDoesNotMatchContext_ReturnsMismatch()
    {
        var service = CreateService(TenantId.New());
        var command = CreateCommand(TenantId.New(), StudentId.New(), UserId.New());

        var result = await service.CreateManualPaymentAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Mismatch");
        _studentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<StudentId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private PaymentService CreateService(TenantId tenantId)
    {
        return new PaymentService(
            _paymentRepository.Object,
            _familyGroupRepository.Object,
            _studentRepository.Object,
            _userRepository.Object,
            _unitOfWork.Object,
            _clock.Object,
            _emailSender.Object,
            new TestTenantContext(tenantId));
    }

    private Payment CreatePayment(TenantId tenantId, StudentId studentId, UserId guardianUserId)
    {
        var money = Money.Create(75.00m, "EUR").Value;
        return Payment.Create(
            tenantId,
            studentId,
            guardianUserId,
            money,
            PaymentMethod.MbWay,
            new DateOnly(2026, 5, 31),
            "May tuition",
            _clock.Object.UtcNow).Value;
    }
}
