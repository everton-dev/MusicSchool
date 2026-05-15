namespace MusicSchool.API.Contracts;

public sealed record TeacherSummaryResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Email,
    IReadOnlyCollection<string> LessonTypes,
    IReadOnlyCollection<TeacherLessonTypeResponse> LessonTypeOptions,
    bool IsAvailable,
    string? AbsenceReason);

public sealed record TeacherLessonTypeResponse(Guid InstrumentId, string Name);

public sealed record TeacherPauseRequest(string Reason);

public sealed record TeacherPauseResponse(
    Guid Id,
    Guid TeacherId,
    string Reason,
    DateTimeOffset StartsOnUtc,
    DateTimeOffset? EndsOnUtc,
    bool IsActive);

public sealed record TeacherScheduleResponse(
    Guid TeacherId,
    IReadOnlyCollection<TeacherScheduleLessonResponse> Lessons,
    IReadOnlyCollection<TeacherPauseResponse> Pauses,
    IReadOnlyCollection<TeacherScheduleStudentResponse> Students,
    bool BillingUpdated = false);

public sealed record TeacherScheduleLessonResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid InstrumentId,
    string InstrumentName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes,
    string RecurrenceRule,
    string Status);

public sealed record TeacherScheduleStudentResponse(
    Guid StudentId,
    Guid UserId,
    string Name);

public sealed record CreateTeacherScheduleLessonRequest(
    Guid TenantId,
    Guid StudentId,
    Guid InstrumentId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string RecurrenceRule);
