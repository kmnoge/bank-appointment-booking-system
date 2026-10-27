using BankAppointments.Api.CQRS.Commands;
using BankAppointments.Api.Services;
using MediatR;
namespace BankAppointments.Api.CQRS.Handlers;
public sealed class CancelAppointmentCommandHandler(IAppointmentCommandService commands) : IRequestHandler<CancelAppointmentCommand, bool>
{ public Task<bool> Handle(CancelAppointmentCommand c, CancellationToken ct) => commands.CancelAsync(c.AppointmentId, ct); }
