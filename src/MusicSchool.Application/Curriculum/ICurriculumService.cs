using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Curriculum;

public interface ICurriculumService
{
    Task<Result<PagedResult<CurriculumNodeDto>>> ListNodesAsync(ListCurriculumNodesQuery query, CancellationToken cancellationToken = default);

    Task<Result<CurriculumNodeDto>> CreateNodeAsync(CreateCurriculumNodeCommand command, CancellationToken cancellationToken = default);

    Task<Result<CurriculumNodeDto>> UploadResourceAsync(UploadCurriculumResourceCommand command, CancellationToken cancellationToken = default);

    Task<Result<ResourceDownloadDto>> CreateResourceDownloadAsync(Guid curriculumNodeId, CancellationToken cancellationToken = default);

    Task<Result<StudentCurriculumProgressDto>> UpdateProgressAsync(UpdateStudentCurriculumProgressCommand command, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<StudentCurriculumProgressDto>>> ListProgressAsync(ListStudentCurriculumProgressQuery query, CancellationToken cancellationToken = default);
}
