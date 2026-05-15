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
        FullAddress = string.Empty;
        PostalCode = string.Empty;
        DocumentType = "CC";
        DocumentNumber = string.Empty;
        ContactPhone = string.Empty;
    }

    private User(
        UserId id,
        TenantId tenantId,
        EmailAddress email,
        string displayName,
        UserRole role,
        string preferredCulture,
        DateTimeOffset createdOnUtc,
        string fullAddress,
        string postalCode,
        string documentType,
        string documentNumber,
        string contactPhone,
        DateOnly? birthDate)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        DisplayName = displayName;
        Role = role;
        PreferredCulture = preferredCulture;
        CreatedOnUtc = createdOnUtc;
        FullAddress = fullAddress;
        PostalCode = postalCode;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        ContactPhone = contactPhone;
        BirthDate = birthDate;
        IsActive = true;
    }

    public TenantId TenantId { get; private set; }

    public EmailAddress Email { get; private set; }

    public string DisplayName { get; private set; }

    public UserRole Role { get; private set; }

    public string PreferredCulture { get; private set; }

    public string FullAddress { get; private set; }

    public string PostalCode { get; private set; }

    public string DocumentType { get; private set; }

    public string DocumentNumber { get; private set; }

    public string ContactPhone { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset CreatedOnUtc { get; private set; }

    public static Result<User> Create(
        TenantId tenantId,
        string email,
        string displayName,
        UserRole role,
        string preferredCulture,
        DateTimeOffset createdOnUtc,
        string fullAddress = "",
        string postalCode = "",
        string documentNumber = "",
        string contactPhone = "",
        string documentType = "CC",
        DateOnly? birthDate = null)
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
            createdOnUtc,
            NormalizeRequiredText(fullAddress, maxLength: 300),
            NormalizeRequiredText(postalCode, maxLength: 20),
            NormalizeDocumentType(documentType),
            NormalizeRequiredText(documentNumber, maxLength: 80),
            NormalizeRequiredText(contactPhone, maxLength: 40),
            birthDate));
    }

    public Result UpdateRegistration(
        string email,
        string displayName,
        UserRole role,
        string fullAddress,
        string postalCode,
        string documentNumber,
        string contactPhone,
        string documentType,
        DateOnly? birthDate)
    {
        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
        {
            return Result.Failure(new Error("User.DisplayNameInvalid", "Display name is required and must not exceed 200 characters."));
        }

        var detailsResult = ValidateRegistrationDetails(fullAddress, postalCode, documentNumber, contactPhone);
        if (detailsResult.IsFailure)
        {
            return detailsResult;
        }

        Email = emailResult.Value;
        DisplayName = displayName.Trim();
        Role = role;
        FullAddress = fullAddress.Trim();
        PostalCode = postalCode.Trim();
        DocumentType = NormalizeDocumentType(documentType);
        DocumentNumber = documentNumber.Trim();
        ContactPhone = contactPhone.Trim();
        BirthDate = birthDate;

        return Result.Success();
    }

    public Result EnsureRegistrationDetails()
    {
        return ValidateRegistrationDetails(FullAddress, PostalCode, DocumentNumber, ContactPhone);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private static string NormalizeCulture(string preferredCulture)
    {
        return string.IsNullOrWhiteSpace(preferredCulture)
            ? "en-US"
            : preferredCulture.Trim();
    }

    private static Result ValidateRegistrationDetails(
        string fullAddress,
        string postalCode,
        string documentNumber,
        string contactPhone)
    {
        if (fullAddress.Length > 300)
        {
            return Result.Failure(new Error("User.FullAddressInvalid", "Full address must not exceed 300 characters."));
        }

        if (postalCode.Length > 20)
        {
            return Result.Failure(new Error("User.PostalCodeInvalid", "Postal code must not exceed 20 characters."));
        }

        if (string.IsNullOrWhiteSpace(documentNumber) || documentNumber.Length > 80)
        {
            return Result.Failure(new Error("User.DocumentNumberInvalid", "Document number is required and must not exceed 80 characters."));
        }

        if (string.IsNullOrWhiteSpace(contactPhone) || contactPhone.Length > 40)
        {
            return Result.Failure(new Error("User.ContactPhoneInvalid", "Contact phone is required and must not exceed 40 characters."));
        }

        return Result.Success();
    }

    private static string NormalizeRequiredText(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    private static string NormalizeDocumentType(string value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? "CC" : value.Trim();
        return trimmed.Length > 32 ? trimmed[..32] : trimmed;
    }
}
