using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Domain.Repositories;

public interface ICurriculumRepository
{
    Task<CurriculumNode?> GetByIdAsync(CurriculumNodeId id, CancellationToken cancellationToken = default);

    Task<PagedResult<CurriculumNode>> ListByTenantAsync(
        TenantId tenantId,
        InstrumentId? instrumentId,
        CurriculumNodeId? parentNodeId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(CurriculumNode node, CancellationToken cancellationToken = default);
}
