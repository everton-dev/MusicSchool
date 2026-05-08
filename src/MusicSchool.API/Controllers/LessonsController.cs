using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Lessons;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Lessons;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOrTeacher)]
[Route("api/lessons")]
public sealed class LessonsController(ILessonSchedulingService lessonSchedulingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<LessonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? teacherId = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] LessonStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await lessonSchedulingService.ListAsync(
            new ListLessonsQuery(teacherId, studentId, status, pageNumber, pageSize),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpGet("{lessonId:guid}")]
    [ProducesResponseType<LessonDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await lessonSchedulingService.GetByIdAsync(lessonId, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType<LessonDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(ScheduleIndividualLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await lessonSchedulingService.ScheduleIndividualLessonAsync(
            new ScheduleIndividualLessonCommand(
                request.TenantId,
                request.TeacherId,
                request.StudentId,
                request.InstrumentId,
                request.DayOfWeek,
                request.StartTime,
                request.DurationMinutes,
                request.TimeZoneId),
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { lessonId = result.Value.Id }, result.Value);
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
