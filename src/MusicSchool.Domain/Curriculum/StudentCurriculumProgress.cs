using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Curriculum;

public sealed class StudentCurriculumProgress : Entity<StudentCurriculumProgressId>
{
    private StudentCurriculumProgress()
        : base(default)
    {
    }

    private StudentCurriculumProgress(
        StudentCurriculumProgressId id,
        TenantId tenantId,
        StudentId studentId,
        CurriculumNodeId curriculumNodeId,
        StudentCurriculumProgressStatus status,
        DateTimeOffset updatedOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        StudentId = studentId;
        CurriculumNodeId = curriculumNodeId;
        Status = status;
        UpdatedOnUtc = updatedOnUtc;
        CompletedOnUtc = status == StudentCurriculumProgressStatus.Completed ? updatedOnUtc : null;
    }

    public TenantId TenantId { get; private set; }

    public StudentId StudentId { get; private set; }

    public CurriculumNodeId CurriculumNodeId { get; private set; }

    public StudentCurriculumProgressStatus Status { get; private set; }

    public DateTimeOffset UpdatedOnUtc { get; private set; }

    public DateTimeOffset? CompletedOnUtc { get; private set; }

    public static Result<StudentCurriculumProgress> Create(
        TenantId tenantId,
        StudentId studentId,
        CurriculumNodeId curriculumNodeId,
        StudentCurriculumProgressStatus status,
        DateTimeOffset updatedOnUtc)
    {
        if (tenantId.Value == Guid.Empty || studentId.Value == Guid.Empty || curriculumNodeId.Value == Guid.Empty)
        {
            return Result<StudentCurriculumProgress>.Failure(new Error("CurriculumProgress.IdentityRequired", "Tenant, student, and curriculum node identifiers are required."));
        }

        if (updatedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result<StudentCurriculumProgress>.Failure(new Error("Time.NotUtc", "Progress timestamp must be in UTC."));
        }

        return Result<StudentCurriculumProgress>.Success(new StudentCurriculumProgress(
            StudentCurriculumProgressId.New(),
            tenantId,
            studentId,
            curriculumNodeId,
            status,
            updatedOnUtc));
    }

    public Result UpdateStatus(StudentCurriculumProgressStatus status, DateTimeOffset updatedOnUtc)
    {
        if (updatedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Progress timestamp must be in UTC."));
        }

        Status = status;
        UpdatedOnUtc = updatedOnUtc;
        CompletedOnUtc = status == StudentCurriculumProgressStatus.Completed ? updatedOnUtc : null;

        return Result.Success();
    }
}
