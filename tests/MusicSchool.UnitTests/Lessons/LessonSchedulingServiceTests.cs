using FluentAssertions;
using Moq;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Lessons;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Repositories;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.UnitTests;

namespace MusicSchool.UnitTests.Lessons;

public sealed class LessonSchedulingServiceTests
{
    private readonly Mock<ILessonRepository> _lessonRepository = new();
    private readonly Mock<ITeacherRepository> _teacherRepository = new();
    private readonly Mock<IStudentRepository> _studentRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();

    public LessonSchedulingServiceTests()
    {
        _clock.Setup(clock => clock.UtcNow).Returns(new DateTimeOffset(2026, 5, 8, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task ScheduleIndividualLessonAsync_WhenValid_AddsLessonAndCommits()
    {
        var tenantId = TenantId.New();
        var instrumentId = InstrumentId.New();
        var teacher = CreateTeacher(tenantId, instrumentId);
        var student = CreateStudent(tenantId);
        SetupTeacherAndStudent(teacher, student);
        var service = CreateService(tenantId);

        var result = await service.ScheduleIndividualLessonAsync(CreateCommand(tenantId, teacher.Id, student.Id, instrumentId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonStatus.Scheduled);
        _lessonRepository.Verify(repository => repository.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleIndividualLessonAsync_WhenTeacherDoesNotTeachInstrument_ReturnsFailure()
    {
        var tenantId = TenantId.New();
        var taughtInstrumentId = InstrumentId.New();
        var requestedInstrumentId = InstrumentId.New();
        var teacher = CreateTeacher(tenantId, taughtInstrumentId);
        var student = CreateStudent(tenantId);
        SetupTeacherAndStudent(teacher, student);
        var service = CreateService(tenantId);

        var result = await service.ScheduleIndividualLessonAsync(CreateCommand(tenantId, teacher.Id, student.Id, requestedInstrumentId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TeacherInstrument.NotTaught");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleIndividualLessonAsync_WhenTeacherHasConflict_ReturnsFailure()
    {
        var tenantId = TenantId.New();
        var instrumentId = InstrumentId.New();
        var teacher = CreateTeacher(tenantId, instrumentId);
        var student = CreateStudent(tenantId);
        SetupTeacherAndStudent(teacher, student);
        _lessonRepository
            .Setup(repository => repository.HasTeacherScheduleConflictAsync(
                tenantId,
                teacher.Id,
                DayOfWeek.Tuesday,
                new TimeOnly(17, 0),
                new TimeOnly(17, 45),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateService(tenantId);

        var result = await service.ScheduleIndividualLessonAsync(CreateCommand(tenantId, teacher.Id, student.Id, instrumentId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lesson.TeacherScheduleConflict");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleIndividualLessonAsync_WhenStudentHasConflict_ReturnsFailure()
    {
        var tenantId = TenantId.New();
        var instrumentId = InstrumentId.New();
        var teacher = CreateTeacher(tenantId, instrumentId);
        var student = CreateStudent(tenantId);
        SetupTeacherAndStudent(teacher, student);
        _lessonRepository
            .Setup(repository => repository.HasStudentScheduleConflictAsync(
                tenantId,
                student.Id,
                DayOfWeek.Tuesday,
                new TimeOnly(17, 0),
                new TimeOnly(17, 45),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = CreateService(tenantId);

        var result = await service.ScheduleIndividualLessonAsync(CreateCommand(tenantId, teacher.Id, student.Id, instrumentId));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lesson.StudentScheduleConflict");
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupTeacherAndStudent(Teacher teacher, Student student)
    {
        _teacherRepository
            .Setup(repository => repository.GetByIdAsync(teacher.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        _studentRepository
            .Setup(repository => repository.GetByIdAsync(student.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);
    }

    private static Teacher CreateTeacher(TenantId tenantId, InstrumentId instrumentId)
    {
        var teacher = Teacher.Create(tenantId, UserId.New(), "Ana Teacher").Value;
        teacher.AddInstrument(instrumentId);

        return teacher;
    }

    private static Student CreateStudent(TenantId tenantId)
    {
        return Student.Create(tenantId, UserId.New(), "Miguel Student").Value;
    }

    private static ScheduleIndividualLessonCommand CreateCommand(TenantId tenantId, TeacherId teacherId, StudentId studentId, InstrumentId instrumentId)
    {
        return new ScheduleIndividualLessonCommand(
            tenantId.Value,
            teacherId.Value,
            studentId.Value,
            instrumentId.Value,
            DayOfWeek.Tuesday,
            new TimeOnly(17, 0),
            45,
            "Europe/Lisbon");
    }

    [Fact]
    public async Task ScheduleIndividualLessonAsync_WhenTenantDoesNotMatchContext_ReturnsMismatch()
    {
        var commandTenantId = TenantId.New();
        var service = CreateService(TenantId.New());

        var result = await service.ScheduleIndividualLessonAsync(CreateCommand(commandTenantId, TeacherId.New(), StudentId.New(), InstrumentId.New()));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Mismatch");
        _teacherRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private LessonSchedulingService CreateService(TenantId tenantId)
    {
        return new LessonSchedulingService(
            _lessonRepository.Object,
            _teacherRepository.Object,
            _studentRepository.Object,
            _unitOfWork.Object,
            _clock.Object,
            new TestTenantContext(tenantId));
    }
}
