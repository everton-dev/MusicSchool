using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Teachers;

public sealed class Teacher : Entity<TeacherId>
{
    private readonly List<TeacherInstrument> _instruments = [];

    private Teacher()
        : base(default)
    {
        DisplayName = string.Empty;
    }

    private Teacher(TeacherId id, TenantId tenantId, UserId userId, string displayName)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DisplayName = displayName;
        IsAvailable = true;
    }

    public TenantId TenantId { get; private set; }

    public UserId UserId { get; private set; }

    public string DisplayName { get; private set; }

    public bool IsAvailable { get; private set; } = true;

    public string? AbsenceReason { get; private set; }

    public IReadOnlyCollection<TeacherInstrument> Instruments => _instruments.AsReadOnly();

    public static Result<Teacher> Create(TenantId tenantId, UserId userId, string displayName)
    {
        if (tenantId.Value == Guid.Empty || userId.Value == Guid.Empty)
        {
            return Result<Teacher>.Failure(new Error("Teacher.IdentityRequired", "Tenant and user identifiers are required."));
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
        {
            return Result<Teacher>.Failure(new Error("Teacher.DisplayNameInvalid", "Teacher display name is required and must not exceed 200 characters."));
        }

        return Result<Teacher>.Success(new Teacher(TeacherId.New(), tenantId, userId, displayName.Trim()));
    }

    public Result AddInstrument(InstrumentId instrumentId)
    {
        if (instrumentId.Value == Guid.Empty)
        {
            return Result.Failure(new Error("Instrument.Required", "Instrument id is required."));
        }

        if (_instruments.Any(instrument => instrument.InstrumentId == instrumentId))
        {
            return Result.Failure(new Error("TeacherInstrument.Duplicate", "Teacher already teaches this instrument."));
        }

        _instruments.Add(new TeacherInstrument(Id, instrumentId));
        return Result.Success();
    }

    public Result ReplaceInstruments(IEnumerable<InstrumentId> instrumentIds)
    {
        var distinctInstrumentIds = instrumentIds
            .Where(instrumentId => instrumentId.Value != Guid.Empty)
            .Distinct()
            .ToArray();

        if (distinctInstrumentIds.Length == 0)
        {
            return Result.Failure(new Error("TeacherInstrument.Required", "At least one lesson type is required for a teacher."));
        }

        _instruments.Clear();
        foreach (var instrumentId in distinctInstrumentIds)
        {
            _instruments.Add(new TeacherInstrument(Id, instrumentId));
        }

        return Result.Success();
    }

    public bool Teaches(InstrumentId instrumentId)
    {
        return _instruments.Any(instrument => instrument.InstrumentId == instrumentId);
    }

    public Result MarkUnavailable(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 300)
        {
            return Result.Failure(new Error("Teacher.AbsenceReasonInvalid", "Absence reason is required and must not exceed 300 characters."));
        }

        IsAvailable = false;
        AbsenceReason = reason.Trim();
        return Result.Success();
    }

    public void MarkAvailable()
    {
        IsAvailable = true;
        AbsenceReason = null;
    }
}
