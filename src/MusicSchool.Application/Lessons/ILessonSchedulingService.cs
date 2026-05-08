using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Lessons;

public interface ILessonSchedulingService
{
    Task<Result<LessonDto>> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<LessonDto>>> ListAsync(ListLessonsQuery query, CancellationToken cancellationToken = default);

    Task<Result<LessonDto>> ScheduleIndividualLessonAsync(ScheduleIndividualLessonCommand command, CancellationToken cancellationToken = default);
}
