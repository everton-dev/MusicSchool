using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOnly)]
[Route("api/teachers")]
public sealed class TeachersController(
    MusicSchoolDbContext dbContext,
    CurrentTenant currentTenant,
    IClock clock) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<TeacherSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var teachers = await dbContext.Teachers
            .Include(teacher => teacher.Instruments)
            .Where(teacher => teacher.TenantId == tenantResult.Value)
            .OrderBy(teacher => teacher.DisplayName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var responses = new List<TeacherSummaryResponse>(teachers.Count);

        foreach (var teacher in teachers)
        {
            responses.Add(await ToTeacherSummaryAsync(teacher, cancellationToken).ConfigureAwait(false));
        }

        return Ok(responses);
    }

    [HttpGet("{teacherId:guid}/schedule")]
    [ProducesResponseType<TeacherScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacherResult = await GetTeacherAsync(teacherId, cancellationToken).ConfigureAwait(false);
        if (teacherResult.IsFailure)
        {
            return ToActionResult(teacherResult);
        }

        return Ok(await ToScheduleResponseAsync(teacherResult.Value, billingUpdated: false, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{teacherId:guid}/schedule")]
    [ProducesResponseType<TeacherScheduleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSchedule(Guid teacherId, CreateTeacherScheduleLessonRequest request, CancellationToken cancellationToken)
    {
        var tenantResult = EnsureTenant(request.TenantId);
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var teacherResult = await GetTeacherAsync(teacherId, cancellationToken).ConfigureAwait(false);
        if (teacherResult.IsFailure)
        {
            return ToActionResult(teacherResult);
        }

        var teacher = teacherResult.Value;
        if (!teacher.IsAvailable || await HasActivePauseAsync(tenantResult.Value, teacher.Id, cancellationToken).ConfigureAwait(false))
        {
            return BadRequest(new ApiErrorResponse("SCHEDULE.CONFLICT", "Teacher has an active pause."));
        }

        var studentId = new StudentId(request.StudentId);
        var student = await dbContext.Students.SingleOrDefaultAsync(
            item => item.TenantId == tenantResult.Value && item.Id == studentId,
            cancellationToken).ConfigureAwait(false);
        if (student is null)
        {
            return NotFound(new ApiErrorResponse("Student.NotFound", "Student was not found."));
        }

        var instrumentId = new InstrumentId(request.InstrumentId);
        var instrument = await dbContext.Instruments.SingleOrDefaultAsync(
            item => item.TenantId == tenantResult.Value && item.Id == instrumentId,
            cancellationToken).ConfigureAwait(false);
        if (instrument is null || !teacher.Teaches(instrumentId))
        {
            return BadRequest(new ApiErrorResponse("TeacherInstrument.NotTaught", "Teacher does not teach the requested instrument."));
        }

        if (request.DurationMinutes is not (30 or 60 or 90 or 120))
        {
            return BadRequest(new ApiErrorResponse("Lesson.DurationInvalid", "Lesson duration must be 30, 60, 90, or 120 minutes."));
        }

        var scheduleResult = WeeklyLessonSchedule.Create(request.DayOfWeek, request.StartTime, request.DurationMinutes, "Europe/Lisbon");
        if (scheduleResult.IsFailure)
        {
            return ToActionResult(scheduleResult);
        }

        var days = ExpandRecurrence(request.RecurrenceRule, request.DayOfWeek);
        foreach (var day in days)
        {
            var conflict = await HasTeacherConflictAsync(
                tenantResult.Value,
                teacher.Id,
                day,
                request.StartTime,
                request.StartTime.AddMinutes(request.DurationMinutes),
                cancellationToken).ConfigureAwait(false);
            if (conflict)
            {
                return BadRequest(new ApiErrorResponse("SCHEDULE.CONFLICT", "Requested slot overlaps another teacher lesson."));
            }
        }

        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken).ConfigureAwait(false);
        foreach (var day in days)
        {
            var dayScheduleResult = WeeklyLessonSchedule.Create(day, request.StartTime, request.DurationMinutes, "Europe/Lisbon");
            if (dayScheduleResult.IsFailure)
            {
                return ToActionResult(dayScheduleResult);
            }

            var lessonResult = Lesson.Create(
                tenantResult.Value,
                teacher.Id,
                student.Id,
                instrumentId,
                dayScheduleResult.Value,
                clock.UtcNow,
                NormalizeRecurrence(request.RecurrenceRule));
            if (lessonResult.IsFailure)
            {
                return ToActionResult(lessonResult);
            }

            await dbContext.Lessons.AddAsync(lessonResult.Value, cancellationToken).ConfigureAwait(false);
        }

        var billingUpdated = await EnsureBillingForAllocationAsync(
            tenantResult.Value,
            teacher,
            student,
            request.DurationMinutes,
            NormalizeRecurrence(request.RecurrenceRule),
            cancellationToken).ConfigureAwait(false);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return CreatedAtAction(
            nameof(GetSchedule),
            new { teacherId = teacher.Id.Value },
            await ToScheduleResponseAsync(teacher, billingUpdated, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{teacherId:guid}/pause")]
    [ProducesResponseType<TeacherPauseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pause(Guid teacherId, TeacherPauseRequest request, CancellationToken cancellationToken)
    {
        var teacherResult = await GetTeacherAsync(teacherId, cancellationToken).ConfigureAwait(false);
        if (teacherResult.IsFailure)
        {
            return ToActionResult(teacherResult);
        }

        var teacher = teacherResult.Value;
        var pauseResult = TeacherPause.Create(teacher.TenantId, teacher.Id, request.Reason, clock.UtcNow);
        if (pauseResult.IsFailure)
        {
            return ToActionResult(pauseResult);
        }

        var unavailableResult = teacher.MarkUnavailable(request.Reason);
        if (unavailableResult.IsFailure)
        {
            return ToActionResult(unavailableResult);
        }

        await dbContext.TeacherPauses.AddAsync(pauseResult.Value, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CreatedAtAction(nameof(GetSchedule), new { teacherId = teacher.Id.Value }, ToPauseResponse(pauseResult.Value));
    }

    private async Task<bool> EnsureBillingForAllocationAsync(
        TenantId tenantId,
        Teacher teacher,
        Student student,
        int durationMinutes,
        string recurrenceRule,
        CancellationToken cancellationToken)
    {
        var guardianUserId = await ResolveBillingGuardianAsync(tenantId, student, cancellationToken).ConfigureAwait(false);
        if (!guardianUserId.HasValue)
        {
            return false;
        }

        var frequencyMultiplier = recurrenceRule.Equals("Daily", StringComparison.OrdinalIgnoreCase) ? 20 : 4;
        var amountResult = Money.Create((durationMinutes / 30m) * 25m * frequencyMultiplier, "EUR");
        if (amountResult.IsFailure)
        {
            return false;
        }

        var description = $"Teacher allocation {teacher.DisplayName} - {student.DisplayName} - {recurrenceRule} {durationMinutes}m";
        var existingPayment = await dbContext.Payments.AnyAsync(
            payment =>
                payment.TenantId == tenantId &&
                payment.StudentId == student.Id &&
                payment.GuardianUserId == guardianUserId.Value &&
                payment.Description == description &&
                payment.Status == PaymentStatus.Pending,
            cancellationToken).ConfigureAwait(false);
        if (existingPayment)
        {
            return false;
        }

        var paymentResult = Payment.Create(
            tenantId,
            student.Id,
            guardianUserId.Value,
            amountResult.Value,
            PaymentMethod.BankTransfer,
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime.Date),
            description,
            clock.UtcNow);
        if (paymentResult.IsFailure)
        {
            return false;
        }

        await dbContext.Payments.AddAsync(paymentResult.Value, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<UserId?> ResolveBillingGuardianAsync(TenantId tenantId, Student student, CancellationToken cancellationToken)
    {
        var studentUser = await dbContext.Users.SingleOrDefaultAsync(
            user => user.TenantId == tenantId && user.Id == student.UserId,
            cancellationToken).ConfigureAwait(false);
        if (studentUser?.Role == UserRole.Guardian)
        {
            return studentUser.Id;
        }

        return await dbContext.FamilyRelationships
            .Where(relationship => relationship.StudentId == student.Id && relationship.IsPrimaryPayer)
            .Select(relationship => (UserId?)relationship.GuardianUserId)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TeacherScheduleResponse> ToScheduleResponseAsync(Teacher teacher, bool billingUpdated, CancellationToken cancellationToken)
    {
        var lessons = await dbContext.Lessons
            .Where(lesson => lesson.TenantId == teacher.TenantId && lesson.TeacherId == teacher.Id)
            .OrderBy(lesson => lesson.Schedule.DayOfWeek)
            .ThenBy(lesson => lesson.Schedule.StartTime)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var pauses = await dbContext.TeacherPauses
            .Where(pause => pause.TenantId == teacher.TenantId && pause.TeacherId == teacher.Id && pause.IsActive)
            .OrderByDescending(pause => pause.StartsOnUtc)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var students = await dbContext.Students
            .Where(student => student.TenantId == teacher.TenantId)
            .OrderBy(student => student.DisplayName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var studentIds = lessons.Select(lesson => lesson.StudentId).Concat(students.Select(student => student.Id)).Distinct().ToArray();
        var studentNames = await dbContext.Students
            .Where(student => student.TenantId == teacher.TenantId && studentIds.Contains(student.Id))
            .ToDictionaryAsync(student => student.Id, student => student.DisplayName, cancellationToken).ConfigureAwait(false);
        var instrumentIds = lessons.Select(lesson => lesson.InstrumentId).Distinct().ToArray();
        var instrumentNames = await dbContext.Instruments
            .Where(instrument => instrument.TenantId == teacher.TenantId && instrumentIds.Contains(instrument.Id))
            .ToDictionaryAsync(instrument => instrument.Id, instrument => instrument.Name, cancellationToken).ConfigureAwait(false);

        return new TeacherScheduleResponse(
            teacher.Id.Value,
            lessons.Select(lesson => new TeacherScheduleLessonResponse(
                lesson.Id.Value,
                lesson.StudentId.Value,
                studentNames.TryGetValue(lesson.StudentId, out var studentName) ? studentName : "Student",
                lesson.InstrumentId.Value,
                instrumentNames.TryGetValue(lesson.InstrumentId, out var instrumentName) ? instrumentName : "Lesson",
                lesson.Schedule.DayOfWeek,
                lesson.Schedule.StartTime,
                lesson.Schedule.EndTime,
                lesson.Schedule.DurationMinutes,
                lesson.RecurrenceRule,
                lesson.Status.ToString())).ToArray(),
            pauses.Select(ToPauseResponse).ToArray(),
            students.Select(student => new TeacherScheduleStudentResponse(student.Id.Value, student.UserId.Value, student.DisplayName)).ToArray(),
            billingUpdated);
    }

    private async Task<TeacherSummaryResponse> ToTeacherSummaryAsync(Teacher teacher, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(
            item => item.TenantId == teacher.TenantId && item.Id == teacher.UserId,
            cancellationToken).ConfigureAwait(false);
        var instrumentIds = teacher.Instruments.Select(instrument => instrument.InstrumentId).ToArray();
        var lessonTypeOptions = await dbContext.Instruments
            .Where(instrument => instrument.TenantId == teacher.TenantId && instrumentIds.Contains(instrument.Id))
            .OrderBy(instrument => instrument.Name)
            .Select(instrument => new TeacherLessonTypeResponse(instrument.Id.Value, instrument.Name))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return new TeacherSummaryResponse(
            teacher.Id.Value,
            teacher.UserId.Value,
            teacher.DisplayName,
            user?.Email.Value ?? string.Empty,
            lessonTypeOptions.Select(item => item.Name).ToArray(),
            lessonTypeOptions,
            teacher.IsAvailable,
            teacher.AbsenceReason);
    }

    private async Task<Result<Teacher>> GetTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return Result<Teacher>.Failure(tenantResult.Error);
        }

        var teacher = await dbContext.Teachers
            .Include(item => item.Instruments)
            .SingleOrDefaultAsync(item => item.TenantId == tenantResult.Value && item.Id == new TeacherId(teacherId), cancellationToken)
            .ConfigureAwait(false);
        return teacher is null
            ? Result<Teacher>.Failure(new Error("Teacher.NotFound", "Teacher was not found."))
            : Result<Teacher>.Success(teacher);
    }

    private async Task<bool> HasTeacherConflictAsync(
        TenantId tenantId,
        TeacherId teacherId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken)
    {
        var lessons = await dbContext.Lessons
            .Where(lesson =>
                lesson.TenantId == tenantId &&
                lesson.TeacherId == teacherId &&
                lesson.Schedule.DayOfWeek == dayOfWeek &&
                lesson.Status != LessonStatus.Cancelled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return lessons.Any(lesson => lesson.Schedule.StartTime < endTime && lesson.Schedule.EndTime > startTime);
    }

    private async Task<bool> HasActivePauseAsync(TenantId tenantId, TeacherId teacherId, CancellationToken cancellationToken)
    {
        return await dbContext.TeacherPauses.AnyAsync(
            pause => pause.TenantId == tenantId && pause.TeacherId == teacherId && pause.IsActive,
            cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyCollection<DayOfWeek> ExpandRecurrence(string recurrenceRule, DayOfWeek dayOfWeek)
    {
        return recurrenceRule.Equals("Daily", StringComparison.OrdinalIgnoreCase)
            ? [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday]
            : [dayOfWeek];
    }

    private static string NormalizeRecurrence(string recurrenceRule)
    {
        return recurrenceRule.Equals("Daily", StringComparison.OrdinalIgnoreCase) ? "Daily" : "Weekly";
    }

    private static TeacherPauseResponse ToPauseResponse(TeacherPause pause)
    {
        return new TeacherPauseResponse(
            pause.Id.Value,
            pause.TeacherId.Value,
            pause.Reason,
            pause.StartsOnUtc,
            pause.EndsOnUtc,
            pause.IsActive);
    }

    private Result<TenantId> GetTenant()
    {
        return currentTenant.TenantId is null
            ? Result<TenantId>.Failure(new Error("Tenant.Missing", "Authenticated tenant context is required."))
            : Result<TenantId>.Success(currentTenant.TenantId.Value);
    }

    private Result<TenantId> EnsureTenant(Guid tenantId)
    {
        var currentTenantResult = GetTenant();
        if (currentTenantResult.IsFailure)
        {
            return currentTenantResult;
        }

        var requestedTenantId = new TenantId(tenantId);
        return requestedTenantId == currentTenantResult.Value
            ? Result<TenantId>.Success(requestedTenantId)
            : Result<TenantId>.Failure(new Error("Tenant.Mismatch", "Requested tenant does not match the authenticated tenant."));
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync(CancellationToken cancellationToken)
    {
        return dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;
    }

    private IActionResult ToActionResult(Result result)
    {
        var response = new ApiErrorResponse(result.Error.Code, result.Error.Message);
        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? NotFound(response)
            : BadRequest(response);
    }
}
