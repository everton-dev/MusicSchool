namespace MusicSchool.Application.Lessons;

public sealed record ScheduleIndividualLessonCommand(
    Guid TenantId,
    Guid TeacherId,
    Guid StudentId,
    Guid InstrumentId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId);
