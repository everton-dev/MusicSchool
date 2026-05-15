using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Teachers;

public sealed class TeacherPause : Entity<TeacherPauseId>
{
    private TeacherPause()
        : base(default)
    {
        Reason = string.Empty;
    }

    private TeacherPause(
        TeacherPauseId id,
        TenantId tenantId,
        TeacherId teacherId,
        string reason,
        DateTimeOffset startsOnUtc,
        DateTimeOffset? endsOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        TeacherId = teacherId;
        Reason = reason;
        StartsOnUtc = startsOnUtc;
        EndsOnUtc = endsOnUtc;
        IsActive = true;
    }

    public TenantId TenantId { get; private set; }

    public TeacherId TeacherId { get; private set; }

    public string Reason { get; private set; }

    public DateTimeOffset StartsOnUtc { get; private set; }

    public DateTimeOffset? EndsOnUtc { get; private set; }

    public bool IsActive { get; private set; }

    public static Result<TeacherPause> Create(
        TenantId tenantId,
        TeacherId teacherId,
        string reason,
        DateTimeOffset startsOnUtc,
        DateTimeOffset? endsOnUtc = null)
    {
        if (tenantId.Value == Guid.Empty || teacherId.Value == Guid.Empty)
        {
            return Result<TeacherPause>.Failure(new Error("TeacherPause.IdentityRequired", "Tenant and teacher identifiers are required."));
        }

        if (startsOnUtc.Offset != TimeSpan.Zero || (endsOnUtc.HasValue && endsOnUtc.Value.Offset != TimeSpan.Zero))
        {
            return Result<TeacherPause>.Failure(new Error("Time.NotUtc", "Pause timestamps must be in UTC."));
        }

        if (endsOnUtc.HasValue && endsOnUtc.Value <= startsOnUtc)
        {
            return Result<TeacherPause>.Failure(new Error("TeacherPause.RangeInvalid", "Pause end must be after the start."));
        }

        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 300)
        {
            return Result<TeacherPause>.Failure(new Error("TeacherPause.ReasonInvalid", "Pause reason is required and must not exceed 300 characters."));
        }

        return Result<TeacherPause>.Success(new TeacherPause(
            TeacherPauseId.New(),
            tenantId,
            teacherId,
            reason.Trim(),
            startsOnUtc,
            endsOnUtc));
    }
}
