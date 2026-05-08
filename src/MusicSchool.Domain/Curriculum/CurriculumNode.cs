using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Curriculum;

public sealed class CurriculumNode : Entity<CurriculumNodeId>
{
    private CurriculumNode()
        : base(default)
    {
        Title = string.Empty;
    }

    private CurriculumNode(
        CurriculumNodeId id,
        TenantId tenantId,
        InstrumentId instrumentId,
        CurriculumNodeId? parentNodeId,
        string title,
        CurriculumNodeType type,
        int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        InstrumentId = instrumentId;
        ParentNodeId = parentNodeId;
        Title = title;
        Type = type;
        SortOrder = sortOrder;
    }

    public TenantId TenantId { get; private set; }

    public InstrumentId InstrumentId { get; private set; }

    public CurriculumNodeId? ParentNodeId { get; private set; }

    public string Title { get; private set; }

    public CurriculumNodeType Type { get; private set; }

    public int SortOrder { get; private set; }

    public string? BlobName { get; private set; }

    public ResourceFileType? ResourceFileType { get; private set; }

    public string? ResourceFileName { get; private set; }

    public string? ResourceContentType { get; private set; }

    public TeacherId? ResourceUploadedByTeacherId { get; private set; }

    public DateTimeOffset? ResourceUploadedOnUtc { get; private set; }

    public static Result<CurriculumNode> Create(
        TenantId tenantId,
        InstrumentId instrumentId,
        CurriculumNodeId? parentNodeId,
        string title,
        CurriculumNodeType type,
        int sortOrder)
    {
        if (tenantId.Value == Guid.Empty || instrumentId.Value == Guid.Empty)
        {
            return Result<CurriculumNode>.Failure(new Error("Curriculum.IdentityRequired", "Tenant and instrument identifiers are required."));
        }

        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
        {
            return Result<CurriculumNode>.Failure(new Error("Curriculum.TitleInvalid", "Curriculum title is required and must not exceed 200 characters."));
        }

        return Result<CurriculumNode>.Success(new CurriculumNode(CurriculumNodeId.New(), tenantId, instrumentId, parentNodeId, title.Trim(), type, sortOrder));
    }

    public Result AttachResource(
        string blobName,
        ResourceFileType fileType,
        string fileName,
        string contentType,
        TeacherId uploadedByTeacherId,
        DateTimeOffset uploadedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(blobName) || blobName.Length > 512)
        {
            return Result.Failure(new Error("Curriculum.BlobNameInvalid", "Resource blob name is required and must not exceed 512 characters."));
        }

        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            return Result.Failure(new Error("Curriculum.FileNameInvalid", "Resource file name is required and must not exceed 255 characters."));
        }

        if (string.IsNullOrWhiteSpace(contentType) || contentType.Length > 100)
        {
            return Result.Failure(new Error("Curriculum.ContentTypeInvalid", "Resource content type is required and must not exceed 100 characters."));
        }

        if (uploadedByTeacherId.Value == Guid.Empty)
        {
            return Result.Failure(new Error("Teacher.Required", "Teacher id is required."));
        }

        if (uploadedOnUtc.Offset != TimeSpan.Zero)
        {
            return Result.Failure(new Error("Time.NotUtc", "Resource upload timestamp must be in UTC."));
        }

        BlobName = blobName.Trim();
        ResourceFileType = fileType;
        ResourceFileName = fileName.Trim();
        ResourceContentType = contentType.Trim();
        ResourceUploadedByTeacherId = uploadedByTeacherId;
        ResourceUploadedOnUtc = uploadedOnUtc;

        return Result.Success();
    }
}
