namespace MusicSchool.API.Auth;

public static class AuthConstants
{
    public const string TenantHeaderName = "X-Tenant-Id";
    public const string TenantClaimType = "tenant_id";

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string AdminOrTeacher = "AdminOrTeacher";
        public const string AdminOrGuardian = "AdminOrGuardian";
        public const string AdminTeacherOrStudent = "AdminTeacherOrStudent";
        public const string AdminTeacherGuardianOrStudent = "AdminTeacherGuardianOrStudent";
    }
}
