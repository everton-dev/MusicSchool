using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Instruments;

public sealed class Instrument : Entity<InstrumentId>
{
    private Instrument()
        : base(default)
    {
        Name = string.Empty;
    }

    private Instrument(InstrumentId id, TenantId tenantId, string name)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
    }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; }

    public static Result<Instrument> Create(TenantId tenantId, string name)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result<Instrument>.Failure(new Error("Tenant.Required", "Tenant id is required."));
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return Result<Instrument>.Failure(new Error("Instrument.NameInvalid", "Instrument name is required and must not exceed 100 characters."));
        }

        return Result<Instrument>.Success(new Instrument(InstrumentId.New(), tenantId, name.Trim()));
    }
}
