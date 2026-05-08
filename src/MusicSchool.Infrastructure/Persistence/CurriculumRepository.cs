using Microsoft.EntityFrameworkCore;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class CurriculumRepository(MusicSchoolDbContext dbContext) : ICurriculumRepository
{
    public Task<CurriculumNode?> GetByIdAsync(CurriculumNodeId id, CancellationToken cancellationToken = default)
    {
        return dbContext.CurriculumNodes.SingleOrDefaultAsync(node => node.Id == id, cancellationToken);
    }

    public async Task<PagedResult<CurriculumNode>> ListByTenantAsync(
        TenantId tenantId,
        InstrumentId? instrumentId,
        CurriculumNodeId? parentNodeId,
        int skip,
        int take,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CurriculumNodes.Where(node => node.TenantId == tenantId);

        if (instrumentId.HasValue)
        {
            query = query.Where(node => node.InstrumentId == instrumentId.Value);
        }

        if (parentNodeId.HasValue)
        {
            query = query.Where(node => node.ParentNodeId == parentNodeId.Value);
        }

        query = query.OrderBy(node => node.SortOrder).ThenBy(node => node.Title);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<CurriculumNode>(items, pageNumber, take, totalCount);
    }

    public async Task AddAsync(CurriculumNode node, CancellationToken cancellationToken = default)
    {
        await dbContext.CurriculumNodes.AddAsync(node, cancellationToken).ConfigureAwait(false);
    }
}
