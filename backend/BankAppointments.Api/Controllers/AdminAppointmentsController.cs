using System.Security.Cryptography;
using System.Text;
using BankAppointments.Api.CQRS.Commands;
using BankAppointments.Api.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAppointments.Api.Controllers;

[ApiController, Route("api/admin/appointments")]
public sealed class AdminAppointmentsController(ISender sender, IConfiguration configuration) : ControllerBase
{
    private bool IsAuthorized()
    {
        var configuredKey = configuration["BranchAdmin:ApiKey"];
        var suppliedKey = Request.Headers["X-Branch-Admin-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configuredKey) || string.IsNullOrWhiteSpace(suppliedKey)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(configuredKey), Encoding.UTF8.GetBytes(suppliedKey));
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? branchId, [FromQuery] DateOnly? date, [FromQuery] string? status, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        if (!IsAuthorized()) return Unauthorized(new ProblemDetails { Title = "Branch administrator access required.", Status = StatusCodes.Status401Unauthorized });
        if (!string.IsNullOrWhiteSpace(status) && !Enum.TryParse<BankAppointments.Api.Models.AppointmentStatus>(status, true, out _))
            return BadRequest(new ProblemDetails { Title = "Status must be Confirmed or Cancelled.", Status = StatusCodes.Status400BadRequest });
        return Ok(await sender.Send(new GetAdminAppointmentsQuery(branchId, date, status, search), cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAuthorized()) return Unauthorized(new ProblemDetails { Title = "Branch administrator access required.", Status = StatusCodes.Status401Unauthorized });
        return await sender.Send(new CancelAppointmentCommand(id), cancellationToken)
            ? NoContent()
            : NotFound(new ProblemDetails { Title = "Appointment was not found.", Status = StatusCodes.Status404NotFound });
    }
}
