using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Application.Lessons;

public sealed class LessonSchedulingService(
    ILessonRepository lessonRepository,
    ITeacherRepository teacherRepository,
    IStudentRepository studentRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    ITenantContext tenantContext) : ILessonSchedulingService
{
    public async Task<Result<LessonDto>> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        if (lessonId == Guid.Empty)
        {
            return Result<LessonDto>.Failure(new Error("Lesson.IdRequired", "Lesson id is required."));
        }

        var lesson = await lessonRepository.GetByIdAsync(new LessonId(lessonId), cancellationToken).ConfigureAwait(false);
        if (lesson is null)
        {
            return Result<LessonDto>.Failure(new Error("Lesson.NotFound", "Lesson was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, lesson.TenantId);
        return tenantResult.IsFailure
            ? Result<LessonDto>.Failure(tenantResult.Error)
            : Result<LessonDto>.Success(lesson.ToDto());
    }

    public async Task<Result<PagedResult<LessonDto>>> ListAsync(ListLessonsQuery query, CancellationToken cancellationToken = default)
    {
        var tenantResult = TenantGuard.GetRequiredTenant(tenantContext);
        if (tenantResult.IsFailure)
        {
            return Result<PagedResult<LessonDto>>.Failure(tenantResult.Error);
        }

        var teacherId = query.TeacherId.HasValue ? new TeacherId(query.TeacherId.Value) : (TeacherId?)null;
        var studentId = query.StudentId.HasValue ? new StudentId(query.StudentId.Value) : (StudentId?)null;
        var pagedLessons = await lessonRepository.ListByTenantAsync(
            tenantResult.Value,
            teacherId,
            studentId,
            query.Status,
            query.Skip,
            query.NormalizedPageSize,
            query.NormalizedPageNumber,
            cancellationToken).ConfigureAwait(false);

        return Result<PagedResult<LessonDto>>.Success(new PagedResult<LessonDto>(
            pagedLessons.Items.Select(lesson => lesson.ToDto()).ToArray(),
            pagedLessons.PageNumber,
            pagedLessons.PageSize,
            pagedLessons.TotalCount));
    }

    public async Task<Result<LessonDto>> ScheduleIndividualLessonAsync(ScheduleIndividualLessonCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var tenantResult = TenantGuard.EnsureTenant(tenantContext, tenantId);
        if (tenantResult.IsFailure)
        {
            return Result<LessonDto>.Failure(tenantResult.Error);
        }

        var teacherId = new TeacherId(command.TeacherId);
        var studentId = new StudentId(command.StudentId);
        var instrumentId = new InstrumentId(command.InstrumentId);

        var scheduleResult = WeeklyLessonSchedule.Create(command.DayOfWeek, command.StartTime, command.DurationMinutes, command.TimeZoneId);
        if (scheduleResult.IsFailure)
        {
            return Result<LessonDto>.Failure(scheduleResult.Error);
        }

        var teacher = await teacherRepository.GetByIdAsync(teacherId, cancellationToken).ConfigureAwait(false);
        if (teacher is null || teacher.TenantId != tenantId)
        {
            return Result<LessonDto>.Failure(new Error("Teacher.NotFound", "Teacher was not found."));
        }

        if (!teacher.Teaches(instrumentId))
        {
            return Result<LessonDto>.Failure(new Error("TeacherInstrument.NotTaught", "Teacher does not teach the requested instrument."));
        }

        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (student is null || student.TenantId != tenantId)
        {
            return Result<LessonDto>.Failure(new Error("Student.NotFound", "Student was not found."));
        }

        var schedule = scheduleResult.Value;
        var teacherHasConflict = await lessonRepository.HasTeacherScheduleConflictAsync(
            tenantId,
            teacherId,
            schedule.DayOfWeek,
            schedule.StartTime,
            schedule.EndTime,
            cancellationToken).ConfigureAwait(false);

        if (teacherHasConflict)
        {
            return Result<LessonDto>.Failure(new Error("Lesson.TeacherScheduleConflict", "Teacher already has a lesson during this time."));
        }

        var studentHasConflict = await lessonRepository.HasStudentScheduleConflictAsync(
            tenantId,
            studentId,
            schedule.DayOfWeek,
            schedule.StartTime,
            schedule.EndTime,
            cancellationToken).ConfigureAwait(false);

        if (studentHasConflict)
        {
            return Result<LessonDto>.Failure(new Error("Lesson.StudentScheduleConflict", "Student already has a lesson during this time."));
        }

        var lessonResult = Lesson.Create(tenantId, teacherId, studentId, instrumentId, schedule, clock.UtcNow);
        if (lessonResult.IsFailure)
        {
            return Result<LessonDto>.Failure(lessonResult.Error);
        }

        await lessonRepository.AddAsync(lessonResult.Value, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<LessonDto>.Success(lessonResult.Value.ToDto());
    }
}
