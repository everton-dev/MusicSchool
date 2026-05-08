using MusicSchool.Domain.Lessons;

namespace MusicSchool.Application.Lessons;

public static class LessonMapper
{
    public static LessonDto ToDto(this Lesson lesson)
    {
        return new LessonDto(
            lesson.Id.Value,
            lesson.TeacherId.Value,
            lesson.StudentId.Value,
            lesson.InstrumentId.Value,
            lesson.Schedule.DayOfWeek,
            lesson.Schedule.StartTime,
            lesson.Schedule.DurationMinutes,
            lesson.Schedule.TimeZoneId,
            lesson.Status);
    }
}
