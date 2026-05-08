namespace MusicSchool.API.Contracts;

public sealed record ScheduleIndividualLessonRequest(
    Guid TenantId,
    Guid TeacherId,
    Guid StudentId,
    Guid InstrumentId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId);
