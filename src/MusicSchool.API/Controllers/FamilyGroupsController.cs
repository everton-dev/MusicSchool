using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Families;
using MusicSchool.Domain.Common;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOnly)]
[Route("api/family-groups")]
public sealed class FamilyGroupsController(IFamilyGroupService familyGroupService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<FamilyGroupDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await familyGroupService.ListAsync(new ListFamilyGroupsQuery(pageNumber, pageSize), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{familyGroupId:guid}")]
    [ProducesResponseType<FamilyGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid familyGroupId, CancellationToken cancellationToken)
    {
        var result = await familyGroupService.GetByIdAsync(familyGroupId, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType<FamilyGroupDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateFamilyGroupRequest request, CancellationToken cancellationToken)
    {
        var result = await familyGroupService.CreateAsync(
            new CreateFamilyGroupCommand(request.TenantId, request.DisplayName),
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { familyGroupId = result.Value.Id }, result.Value);
    }

    [HttpPost("{familyGroupId:guid}/relationships")]
    [ProducesResponseType<FamilyGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRelationship(Guid familyGroupId, AddFamilyRelationshipRequest request, CancellationToken cancellationToken)
    {
        var result = await familyGroupService.AddRelationshipAsync(
            new AddFamilyRelationshipCommand(
                familyGroupId,
                request.GuardianUserId,
                request.StudentId,
                request.Kind,
                request.IsPrimaryPayer),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpPost("{familyGroupId:guid}/primary-payer")]
    [ProducesResponseType<FamilyGroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPrimaryPayer(Guid familyGroupId, AssignPrimaryPayerRequest request, CancellationToken cancellationToken)
    {
        var result = await familyGroupService.AssignPrimaryPayerAsync(
            new AssignPrimaryPayerCommand(familyGroupId, request.GuardianUserId, request.StudentId),
            cancellationToken).ConfigureAwait(false);

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
