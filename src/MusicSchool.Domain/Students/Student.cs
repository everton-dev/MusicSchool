using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Students;

public sealed class Student : Entity<StudentId>
{
    private Student()
        : base(default)
    {
        DisplayName = string.Empty;
    }

    private Student(StudentId id, TenantId tenantId, UserId userId, string displayName, DateOnly? birthDate)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DisplayName = displayName;
        BirthDate = birthDate;
    }

    public TenantId TenantId { get; private set; }

    public UserId UserId { get; private set; }

    public string DisplayName { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public static Result<Student> Create(TenantId tenantId, UserId userId, string displayName, DateOnly? birthDate = null)
    {
        if (tenantId.Value == Guid.Empty || userId.Value == Guid.Empty)
        {
            return Result<Student>.Failure(new Error("Student.IdentityRequired", "Tenant and user identifiers are required."));
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
        {
            return Result<Student>.Failure(new Error("Student.DisplayNameInvalid", "Student display name is required and must not exceed 200 characters."));
        }

        return Result<Student>.Success(new Student(StudentId.New(), tenantId, userId, displayName.Trim(), birthDate));
    }
}
