using MusicSchool.Domain.Lessons;

namespace MusicSchool.Application.Lessons;

public sealed record LessonDto(
    Guid Id,
    Guid TeacherId,
    Guid StudentId,
    Guid InstrumentId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId,
    LessonStatus Status);
