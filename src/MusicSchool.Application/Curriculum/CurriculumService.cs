using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Repositories;

namespace MusicSchool.Application.Curriculum;

public sealed class CurriculumService(
    ICurriculumRepository curriculumRepository,
    IStudentRepository studentRepository,
    IStudentCurriculumProgressRepository progressRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    IClock clock,
    ITenantContext tenantContext) : ICurriculumService
{
    private static readonly TimeSpan DownloadLifetime = TimeSpan.FromMinutes(15);

    public async Task<Result<PagedResult<CurriculumNodeDto>>> ListNodesAsync(ListCurriculumNodesQuery query, CancellationToken cancellationToken = default)
    {
        var tenantResult = TenantGuard.GetRequiredTenant(tenantContext);
        if (tenantResult.IsFailure)
        {
            return Result<PagedResult<CurriculumNodeDto>>.Failure(tenantResult.Error);
        }

        var instrumentId = query.InstrumentId.HasValue ? new InstrumentId(query.InstrumentId.Value) : (InstrumentId?)null;
        var parentNodeId = query.ParentNodeId.HasValue ? new CurriculumNodeId(query.ParentNodeId.Value) : (CurriculumNodeId?)null;
        var pagedNodes = await curriculumRepository.ListByTenantAsync(
            tenantResult.Value,
            instrumentId,
            parentNodeId,
            query.Skip,
            query.NormalizedPageSize,
            query.NormalizedPageNumber,
            cancellationToken).ConfigureAwait(false);

        return Result<PagedResult<CurriculumNodeDto>>.Success(new PagedResult<CurriculumNodeDto>(
            pagedNodes.Items.Select(node => node.ToDto()).ToArray(),
            pagedNodes.PageNumber,
            pagedNodes.PageSize,
            pagedNodes.TotalCount));
    }

    public async Task<Result<CurriculumNodeDto>> CreateNodeAsync(CreateCurriculumNodeCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(command.TenantId);
        var tenantResult = TenantGuard.EnsureTenant(tenantContext, tenantId);
        if (tenantResult.IsFailure)
        {
            return Result<CurriculumNodeDto>.Failure(tenantResult.Error);
        }

        var parentNodeId = command.ParentNodeId.HasValue ? new CurriculumNodeId(command.ParentNodeId.Value) : (CurriculumNodeId?)null;
        var nodeResult = CurriculumNode.Create(
            tenantId,
            new InstrumentId(command.InstrumentId),
            parentNodeId,
            command.Title,
            command.Type,
            command.SortOrder);

        if (nodeResult.IsFailure)
        {
            return Result<CurriculumNodeDto>.Failure(nodeResult.Error);
        }

        await curriculumRepository.AddAsync(nodeResult.Value, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<CurriculumNodeDto>.Success(nodeResult.Value.ToDto());
    }

    public async Task<Result<CurriculumNodeDto>> UploadResourceAsync(UploadCurriculumResourceCommand command, CancellationToken cancellationToken = default)
    {
        var node = await curriculumRepository.GetByIdAsync(new CurriculumNodeId(command.CurriculumNodeId), cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return Result<CurriculumNodeDto>.Failure(new Error("Curriculum.NotFound", "Curriculum node was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, node.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<CurriculumNodeDto>.Failure(tenantResult.Error);
        }

        var fileTypeResult = ResolveResourceFileType(command.FileName, command.ContentType);
        if (fileTypeResult.IsFailure)
        {
            return Result<CurriculumNodeDto>.Failure(fileTypeResult.Error);
        }

        var blobName = CreateBlobName(node.TenantId, node.Id, command.FileName);
        var uploadedBlobName = await blobStorageService.UploadAsync(command.Content, blobName, command.ContentType, cancellationToken).ConfigureAwait(false);

        var attachResult = node.AttachResource(
            uploadedBlobName,
            fileTypeResult.Value,
            command.FileName,
            command.ContentType,
            new TeacherId(command.UploadedByTeacherId),
            clock.UtcNow);

        if (attachResult.IsFailure)
        {
            return Result<CurriculumNodeDto>.Failure(attachResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<CurriculumNodeDto>.Success(node.ToDto());
    }

    public async Task<Result<ResourceDownloadDto>> CreateResourceDownloadAsync(Guid curriculumNodeId, CancellationToken cancellationToken = default)
    {
        var node = await curriculumRepository.GetByIdAsync(new CurriculumNodeId(curriculumNodeId), cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return Result<ResourceDownloadDto>.Failure(new Error("Curriculum.NotFound", "Curriculum node was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, node.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<ResourceDownloadDto>.Failure(tenantResult.Error);
        }

        if (string.IsNullOrWhiteSpace(node.BlobName))
        {
            return Result<ResourceDownloadDto>.Failure(new Error("Curriculum.ResourceMissing", "Curriculum node does not have a resource."));
        }

        var uri = await blobStorageService.CreateReadUriAsync(node.BlobName, DownloadLifetime, cancellationToken).ConfigureAwait(false);
        return Result<ResourceDownloadDto>.Success(new ResourceDownloadDto(uri, clock.UtcNow.Add(DownloadLifetime)));
    }

    public async Task<Result<StudentCurriculumProgressDto>> UpdateProgressAsync(UpdateStudentCurriculumProgressCommand command, CancellationToken cancellationToken = default)
    {
        var studentId = new StudentId(command.StudentId);
        var nodeId = new CurriculumNodeId(command.CurriculumNodeId);
        var node = await curriculumRepository.GetByIdAsync(nodeId, cancellationToken).ConfigureAwait(false);
        if (node is null)
        {
            return Result<StudentCurriculumProgressDto>.Failure(new Error("Curriculum.NotFound", "Curriculum node was not found."));
        }

        var tenantResult = TenantGuard.EnsureTenant(tenantContext, node.TenantId);
        if (tenantResult.IsFailure)
        {
            return Result<StudentCurriculumProgressDto>.Failure(tenantResult.Error);
        }

        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (student is null || student.TenantId != node.TenantId)
        {
            return Result<StudentCurriculumProgressDto>.Failure(new Error("Student.NotFound", "Student was not found."));
        }

        var progress = await progressRepository.GetAsync(studentId, nodeId, cancellationToken).ConfigureAwait(false);
        if (progress is null)
        {
            var createResult = StudentCurriculumProgress.Create(node.TenantId, studentId, nodeId, command.Status, clock.UtcNow);
            if (createResult.IsFailure)
            {
                return Result<StudentCurriculumProgressDto>.Failure(createResult.Error);
            }

            progress = createResult.Value;
            await progressRepository.AddAsync(progress, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var updateResult = progress.UpdateStatus(command.Status, clock.UtcNow);
            if (updateResult.IsFailure)
            {
                return Result<StudentCurriculumProgressDto>.Failure(updateResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<StudentCurriculumProgressDto>.Success(progress.ToDto());
    }

    public async Task<Result<PagedResult<StudentCurriculumProgressDto>>> ListProgressAsync(ListStudentCurriculumProgressQuery query, CancellationToken cancellationToken = default)
    {
        var tenantResult = TenantGuard.GetRequiredTenant(tenantContext);
        if (tenantResult.IsFailure)
        {
            return Result<PagedResult<StudentCurriculumProgressDto>>.Failure(tenantResult.Error);
        }

        var studentId = new StudentId(query.StudentId);
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (student is null || student.TenantId != tenantResult.Value)
        {
            return Result<PagedResult<StudentCurriculumProgressDto>>.Failure(new Error("Student.NotFound", "Student was not found."));
        }

        var pagedProgress = await progressRepository.ListByStudentAsync(
            tenantResult.Value,
            studentId,
            query.Skip,
            query.NormalizedPageSize,
            query.NormalizedPageNumber,
            cancellationToken).ConfigureAwait(false);

        return Result<PagedResult<StudentCurriculumProgressDto>>.Success(new PagedResult<StudentCurriculumProgressDto>(
            pagedProgress.Items.Select(progress => progress.ToDto()).ToArray(),
            pagedProgress.PageNumber,
            pagedProgress.PageSize,
            pagedProgress.TotalCount));
    }

    private static Result<ResourceFileType> ResolveResourceFileType(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var normalizedContentType = contentType.Trim().ToLowerInvariant();

        if (extension == ".pdf" && normalizedContentType == "application/pdf")
        {
            return Result<ResourceFileType>.Success(ResourceFileType.Pdf);
        }

        if (extension == ".mp3" && normalizedContentType is "audio/mpeg" or "audio/mp3")
        {
            return Result<ResourceFileType>.Success(ResourceFileType.Mp3);
        }

        return Result<ResourceFileType>.Failure(new Error("Curriculum.ResourceTypeUnsupported", "Only PDF and MP3 curriculum resources are supported."));
    }

    private static string CreateBlobName(TenantId tenantId, CurriculumNodeId nodeId, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{tenantId.Value:N}/curriculum/{nodeId.Value:N}/{Guid.NewGuid():N}{extension}";
    }
}
