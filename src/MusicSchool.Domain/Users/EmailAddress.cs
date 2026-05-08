using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Users;

public sealed record EmailAddress
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<EmailAddress> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<EmailAddress>.Failure(new Error("Email.Empty", "Email address is required."));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 256 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            return Result<EmailAddress>.Failure(new Error("Email.Invalid", "Email address is not valid."));
        }

        return Result<EmailAddress>.Success(new EmailAddress(normalized));
    }

    public override string ToString() => Value;
}
