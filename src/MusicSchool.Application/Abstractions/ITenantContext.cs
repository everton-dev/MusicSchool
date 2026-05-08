using MusicSchool.Domain.Common;

namespace MusicSchool.Application.Abstractions;

public interface ITenantContext
{
    TenantId? TenantId { get; }
}
