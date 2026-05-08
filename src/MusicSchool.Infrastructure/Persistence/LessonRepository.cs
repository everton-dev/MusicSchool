using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class LessonRepository(MusicSchoolDbContext dbContext) : ILessonRepository
{
    public Task<Lesson?> GetByIdAsync(LessonId id, CancellationToken cancellationToken = default)
    {
        return dbContext.Lessons.SingleOrDefaultAsync(lesson => lesson.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Lesson>> ListByTenantAsync(
        TenantId tenantId,
        TeacherId? teacherId,
        StudentId? studentId,
        LessonStatus? status,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Lessons.Where(lesson => lesson.TenantId == tenantId);

        if (teacherId.HasValue)
        {
            query = query.Where(lesson => lesson.TeacherId == teacherId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(lesson => lesson.StudentId == studentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(lesson => lesson.Status == status.Value);
        }

        query = query
            .OrderBy(lesson => lesson.Schedule.DayOfWeek)
            .ThenBy(lesson => lesson.Schedule.StartTime);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<Lesson>(items, pageNumber, take, totalCount);
    }

    public async Task<bool> HasTeacherScheduleConflictAsync(
        TenantId tenantId,
        TeacherId teacherId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var lessons = await dbContext.Lessons
            .Where(lesson =>
                lesson.TenantId == tenantId &&
                lesson.TeacherId == teacherId &&
                lesson.Schedule.DayOfWeek == dayOfWeek &&
                lesson.Status != LessonStatus.Cancelled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return lessons.Any(lesson => Overlaps(lesson.Schedule.StartTime, lesson.Schedule.EndTime, startTime, endTime));
    }

    public async Task<bool> HasStudentScheduleConflictAsync(
        TenantId tenantId,
        StudentId studentId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var lessons = await dbContext.Lessons
            .Where(lesson =>
                lesson.TenantId == tenantId &&
                lesson.StudentId == studentId &&
                lesson.Schedule.DayOfWeek == dayOfWeek &&
                lesson.Status != LessonStatus.Cancelled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return lessons.Any(lesson => Overlaps(lesson.Schedule.StartTime, lesson.Schedule.EndTime, startTime, endTime));
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await dbContext.Lessons.AddAsync(lesson, cancellationToken).ConfigureAwait(false);
    }

    private static bool Overlaps(TimeOnly existingStart, TimeOnly existingEnd, TimeOnly requestedStart, TimeOnly requestedEnd)
    {
        return existingStart < requestedEnd && existingEnd > requestedStart;
    }
}
