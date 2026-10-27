using BankAppointments.Api.DTOs;
using BankAppointments.Api.CQRS.Commands;
using BankAppointments.Api.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BankAppointments.Api.Controllers;
[ApiController, Route("api/appointments")]
public sealed class AppointmentsController(ISender sender) : ControllerBase
{
    [HttpGet("branches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken) => Ok(await sender.Send(new GetBranchesQuery(), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateAppointmentCommand(request), cancellationToken);
        return Created($"/api/appointments/{result.Id}", result);
    }
}
