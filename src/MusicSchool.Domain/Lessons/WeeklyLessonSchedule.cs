using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Lessons;

public sealed record WeeklyLessonSchedule
{
    private WeeklyLessonSchedule(DayOfWeek dayOfWeek, TimeOnly startTime, int durationMinutes, string timeZoneId)
    {
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        TimeZoneId = timeZoneId;
    }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartTime { get; }

    public int DurationMinutes { get; }

    public string TimeZoneId { get; }

    public TimeOnly EndTime => StartTime.AddMinutes(DurationMinutes);

    public static Result<WeeklyLessonSchedule> Create(
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        int durationMinutes,
        string timeZoneId)
    {
        if (durationMinutes is < 15 or > 180)
        {
            return Result<WeeklyLessonSchedule>.Failure(new Error("Lesson.DurationInvalid", "Lesson duration must be between 15 and 180 minutes."));
        }

        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Length > 100)
        {
            return Result<WeeklyLessonSchedule>.Failure(new Error("Lesson.TimeZoneInvalid", "Lesson time zone is required and must not exceed 100 characters."));
        }

        return Result<WeeklyLessonSchedule>.Success(new WeeklyLessonSchedule(dayOfWeek, startTime, durationMinutes, timeZoneId.Trim()));
    }
}
