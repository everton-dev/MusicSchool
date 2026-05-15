namespace MusicSchool.Domain.Common;

public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());
}

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
}

public readonly record struct StudentId(Guid Value)
{
    public static StudentId New() => new(Guid.NewGuid());
}

public readonly record struct TeacherId(Guid Value)
{
    public static TeacherId New() => new(Guid.NewGuid());
}

public readonly record struct TeacherPauseId(Guid Value)
{
    public static TeacherPauseId New() => new(Guid.NewGuid());
}

public readonly record struct InstrumentId(Guid Value)
{
    public static InstrumentId New() => new(Guid.NewGuid());
}

public readonly record struct FamilyGroupId(Guid Value)
{
    public static FamilyGroupId New() => new(Guid.NewGuid());
}

public readonly record struct FamilyRelationshipId(Guid Value)
{
    public static FamilyRelationshipId New() => new(Guid.NewGuid());
}

public readonly record struct LessonId(Guid Value)
{
    public static LessonId New() => new(Guid.NewGuid());
}

public readonly record struct CurriculumNodeId(Guid Value)
{
    public static CurriculumNodeId New() => new(Guid.NewGuid());
}

public readonly record struct StudentCurriculumProgressId(Guid Value)
{
    public static StudentCurriculumProgressId New() => new(Guid.NewGuid());
}

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
}
