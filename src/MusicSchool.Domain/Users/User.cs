using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Users;

public sealed class User : Entity<UserId>
{
    private User()
        : base(default)
    {
        Email = null!;
        DisplayName = string.Empty;
        PreferredCulture = "en-US";
    }

    private User(
        UserId id,
        TenantId tenantId,
        EmailAddress email,
        string displayName,
        UserRole role,
        string preferredCulture,
        DateTimeOffset createdOnUtc)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        DisplayName = displayName;
        Role = role;
        PreferredCulture = preferredCulture;
        CreatedOnUtc = createdOnUtc;
    }

    public TenantId TenantId { get; private set; }

    public EmailAddress Email { get; private set; }

    public string DisplayName { get; private set; }

    public UserRole Role { get; private set; }

    public string PreferredCulture { get; private set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public static Result<User> Create(
        TenantId tenantId,
        string email,
        string displayName,
        UserRole role,
        string preferredCulture,
        DateTimeOffset createdOnUtc)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result<User>.Failure(new Error("Tenant.Required", "Tenant id is required."));
        }

        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<User>.Failure(emailResult.Error);
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
        {
            return Result<User>.Failure(new Error("User.DisplayNameInvalid", "Display name is required and must not exceed 200 characters."));
        }

        if (createdOnUtc.Offset != TimeSpan.Zero)
        {
            return Result<User>.Failure(new Error("Time.NotUtc", "Created timestamp must be in UTC."));
        }

        return Result<User>.Success(new User(
            UserId.New(),
            tenantId,
            emailResult.Value,
            displayName.Trim(),
            role,
            NormalizeCulture(preferredCulture),
            createdOnUtc));
    }

    private static string NormalizeCulture(string preferredCulture)
    {
        return string.IsNullOrWhiteSpace(preferredCulture)
            ? "en-US"
            : preferredCulture.Trim();
    }
}
