using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Curriculum;
using MusicSchool.Domain.Common;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOrTeacher)]
[Route("api/curriculum-nodes")]
public sealed class CurriculumController(ICurriculumService curriculumService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<CurriculumNodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? instrumentId = null,
        [FromQuery] Guid? parentNodeId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await curriculumService.ListNodesAsync(
            new ListCurriculumNodesQuery(instrumentId, parentNodeId, pageNumber, pageSize),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType<CurriculumNodeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateCurriculumNodeRequest request, CancellationToken cancellationToken)
    {
        var result = await curriculumService.CreateNodeAsync(
            new CreateCurriculumNodeCommand(
                request.TenantId,
                request.InstrumentId,
                request.ParentNodeId,
                request.Title,
                request.Type,
                request.SortOrder),
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return Created($"/api/curriculum-nodes/{result.Value.Id}", result.Value);
    }

    [HttpPost("{curriculumNodeId:guid}/resources")]
    [RequestSizeLimit(50_000_000)]
    [ProducesResponseType<CurriculumNodeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadResource(
        Guid curriculumNodeId,
        [FromForm] Guid uploadedByTeacherId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("Curriculum.FileEmpty", "Resource file is required."));
        }

        await using var content = file.OpenReadStream();
        var result = await curriculumService.UploadResourceAsync(
            new UploadCurriculumResourceCommand(
                curriculumNodeId,
                uploadedByTeacherId,
                file.FileName,
                file.ContentType,
                content),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpPost("{curriculumNodeId:guid}/resources/download-url")]
    [ProducesResponseType<ResourceDownloadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDownloadUrl(Guid curriculumNodeId, CancellationToken cancellationToken)
    {
        var result = await curriculumService.CreateResourceDownloadAsync(curriculumNodeId, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var response = new ApiErrorResponse(result.Error.Code, result.Error.Message);
        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? NotFound(response)
            : BadRequest(response);
    }
}
