using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;

namespace MusicSchool.Domain.Repositories;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(LessonId id, CancellationToken cancellationToken = default);

    Task<PagedResult<Lesson>> ListByTenantAsync(
        TenantId tenantId,
        TeacherId? teacherId,
        StudentId? studentId,
        LessonStatus? status,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<bool> HasTeacherScheduleConflictAsync(
        TenantId tenantId,
        TeacherId teacherId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default);

    Task<bool> HasStudentScheduleConflictAsync(
        TenantId tenantId,
        StudentId studentId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default);

    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
}
