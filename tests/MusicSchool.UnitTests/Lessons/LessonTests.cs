using FluentAssertions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;

namespace MusicSchool.UnitTests.Lessons;

public sealed class LessonTests
{
    [Fact]
    public void Create_WithUtcTimestamp_CreatesScheduledIndividualLesson()
    {
        var schedule = WeeklyLessonSchedule.Create(DayOfWeek.Tuesday, new TimeOnly(17, 30), 45, "Europe/Lisbon").Value;

        var result = Lesson.Create(
            TenantId.New(),
            TeacherId.New(),
            StudentId.New(),
            InstrumentId.New(),
            schedule,
            DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonStatus.Scheduled);
        result.Value.Schedule.DurationMinutes.Should().Be(45);
    }

    [Fact]
    public void Create_WithNonUtcTimestamp_Fails()
    {
        var schedule = WeeklyLessonSchedule.Create(DayOfWeek.Tuesday, new TimeOnly(17, 30), 45, "Europe/Lisbon").Value;
        var nonUtcTimestamp = new DateTimeOffset(2026, 5, 8, 17, 30, 0, TimeSpan.FromHours(1));

        var result = Lesson.Create(
            TenantId.New(),
            TeacherId.New(),
            StudentId.New(),
            InstrumentId.New(),
            schedule,
            nonUtcTimestamp);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Time.NotUtc");
    }

    [Fact]
    public void Reschedule_AfterCancellation_Fails()
    {
        var schedule = WeeklyLessonSchedule.Create(DayOfWeek.Tuesday, new TimeOnly(17, 30), 45, "Europe/Lisbon").Value;
        var lesson = Lesson.Create(TenantId.New(), TeacherId.New(), StudentId.New(), InstrumentId.New(), schedule, DateTimeOffset.UtcNow).Value;
        var newSchedule = WeeklyLessonSchedule.Create(DayOfWeek.Wednesday, new TimeOnly(18, 00), 45, "Europe/Lisbon").Value;

        lesson.Cancel("Student moved away.", DateTimeOffset.UtcNow);
        var result = lesson.Reschedule(newSchedule, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lesson.Cancelled");
    }
}
