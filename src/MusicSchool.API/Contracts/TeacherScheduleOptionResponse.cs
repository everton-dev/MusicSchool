namespace MusicSchool.API.Contracts;

public sealed record TeacherScheduleOptionResponse(
    Guid InstrumentId,
    string InstrumentName,
    Guid TeacherId,
    string TeacherName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    string TimeZoneId,
    bool IsTaken,
    Guid? AssignedStudentId,
    string? AssignedStudentName);
