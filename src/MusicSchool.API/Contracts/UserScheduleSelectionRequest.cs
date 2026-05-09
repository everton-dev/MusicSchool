namespace MusicSchool.API.Contracts;

public sealed record UserScheduleSelectionRequest(
    Guid TeacherId,
    Guid InstrumentId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId);
