using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Curriculum;
using MusicSchool.Domain.Common;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminTeacherOrStudent)]
[Route("api/students/{studentId:guid}/curriculum")]
public sealed class StudentCurriculumProgressController(ICurriculumService curriculumService) : ControllerBase
{
    [HttpGet("progress")]
    [ProducesResponseType<PagedResult<StudentCurriculumProgressDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListProgress(
        Guid studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await curriculumService.ListProgressAsync(
            new ListStudentCurriculumProgressQuery(studentId, pageNumber, pageSize),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpPut("{curriculumNodeId:guid}/progress")]
    [ProducesResponseType<StudentCurriculumProgressDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProgress(
        Guid studentId,
        Guid curriculumNodeId,
        UpdateStudentCurriculumProgressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await curriculumService.UpdateProgressAsync(
            new UpdateStudentCurriculumProgressCommand(studentId, curriculumNodeId, request.Status),
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
