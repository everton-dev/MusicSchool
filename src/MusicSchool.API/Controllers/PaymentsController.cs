using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Payments;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Payments;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOrGuardian)]
[Route("api/payments")]
public sealed class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<PaymentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? guardianUserId = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentService.ListAsync(
            new ListPaymentsQuery(guardianUserId, studentId, status, pageNumber, pageSize),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid paymentId, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetByIdAsync(paymentId, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateManualPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await paymentService.CreateManualPaymentAsync(
            new CreateManualPaymentCommand(
                request.TenantId,
                request.StudentId,
                request.GuardianUserId,
                request.Amount,
                request.Currency,
                request.Method,
                request.DueDate,
                request.Description),
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { paymentId = result.Value.Id }, result.Value);
    }

    [HttpPost("{paymentId:guid}/confirm")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await paymentService.ConfirmPaymentAsync(
            new ConfirmPaymentCommand(paymentId, request.PaymentReference),
            cancellationToken).ConfigureAwait(false);

        return ToActionResult(result);
    }

    [HttpPost("{paymentId:guid}/reject")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid paymentId, RejectPaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await paymentService.RejectPaymentAsync(
            new RejectPaymentCommand(paymentId, request.Reason),
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
