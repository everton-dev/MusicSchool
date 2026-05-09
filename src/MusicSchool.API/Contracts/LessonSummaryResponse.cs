namespace MusicSchool.API.Contracts;

public sealed record LessonSummaryResponse(
    Guid Id,
    Guid TeacherId,
    Guid StudentId,
    Guid InstrumentId,
    string DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId,
    string Status);
