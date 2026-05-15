using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Lessons;

public sealed class Lesson : Entity<LessonId>
{
    private Lesson()
        : base(default)
    {
        Schedule = null!;
    }

    private Lesson(
        LessonId id,
        TenantId tenantId,
        TeacherId teacherId,
        StudentId studentId,
        InstrumentId instrumentId,
        WeeklyLessonSchedule schedule,
        string recurrenceRule,
        DateTimeOffset createdOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        TeacherId = teacherId;
        StudentId = studentId;
        InstrumentId = instrumentId;
        Schedule = schedule;
        RecurrenceRule = recurrenceRule;
        Status = LessonStatus.Scheduled;
        CreatedOnUtc = createdOnUtc;
    }

    public TenantId TenantId { get; private set; }

    public TeacherId TeacherId { get; private set; }

    public StudentId StudentId { get; private set; }

    public InstrumentId InstrumentId { get; private set; }

    public WeeklyLessonSchedule Schedule { get; private set; }

    public string RecurrenceRule { get; private set; } = "Weekly";

    public LessonStatus Status { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public DateTimeOffset? CancelledOnUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTimeOffset? LastScheduleChangeOnUtc { get; private set; }

    public static Result<Lesson> Create(
        TenantId tenantId,
        TeacherId teacherId,
        StudentId studentId,
        InstrumentId instrumentId,
        WeeklyLessonSchedule schedule,
        DateTimeOffset createdOnUtc,
        string recurrenceRule = "Weekly")
    {
        if (tenantId.Value == Guid.Empty ||
            teacherId.Value == Guid.Empty ||
            studentId.Value == Guid.Empty ||
            instrumentId.Value == Guid.Empty)
        {
            return Result<Lesson>.Failure(new Error("Lesson.IdentityRequired", "Tenant, teacher, student, and instrument identifiers are required."));
        }

        if (createdOnUtc.Offset != TimeSpan.Zero)
        {
            return Result<Lesson>.Failure(new Error("Time.NotUtc", "Created timestamp must be in UTC."));
        }

        var normalizedRecurrence = string.IsNullOrWhiteSpace(recurrenceRule) ? "Weekly" : recurrenceRule.Trim();
        if (normalizedRecurrence.Length > 32)
        {
            return Result<Lesson>.Failure(new Error("Lesson.RecurrenceInvalid", "Lesson recurrence must not exceed 32 characters."));
        }

        return Result<Lesson>.Success(new Lesson(LessonId.New(), tenantId, teacherId, studentId, instrumentId, schedule, normalizedRecurrence, createdOnUtc));
    }

    public Result Reschedule(WeeklyLessonSchedule newSchedule, DateTimeOffset changedOnUtc)
    {
        if (Status == LessonStatus.Cancelled)
        {
            return Result.Failure(new Error("Lesson.Cancelled", "A cancelled lesson cannot be rescheduled."));
        }

        if (changedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Schedule change timestamp must be in UTC."));
        }

        Schedule = newSchedule;
        LastScheduleChangeOnUtc = changedOnUtc;

        return Result.Success();
    }

    public Result Pause()
    {
        if (Status == LessonStatus.Cancelled)
        {
            return Result.Failure(new Error("Lesson.Cancelled", "A cancelled lesson cannot be paused."));
        }

        Status = LessonStatus.Paused;
        return Result.Success();
    }

    public Result Resume()
    {
        if (Status == LessonStatus.Cancelled)
        {
            return Result.Failure(new Error("Lesson.Cancelled", "A cancelled lesson cannot be resumed."));
        }

        Status = LessonStatus.Scheduled;
        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset cancelledOnUtc)
    {
        if (Status == LessonStatus.Cancelled)
        {
            return Result.Failure(new Error("Lesson.AlreadyCancelled", "Lesson is already cancelled."));
        }

        if (cancelledOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Cancellation timestamp must be in UTC."));
        }

        if (!string.IsNullOrWhiteSpace(reason) && reason.Length > 500)
        {
            return Result.Failure(new Error("Lesson.CancellationReasonTooLong", "Cancellation reason must not exceed 500 characters."));
        }

        Status = LessonStatus.Cancelled;
        CancelledOnUtc = cancelledOnUtc;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        return Result.Success();
    }
}
