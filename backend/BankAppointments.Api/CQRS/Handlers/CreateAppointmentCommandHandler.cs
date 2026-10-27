using BankAppointments.Api.CQRS.Commands;
using BankAppointments.Api.DTOs;
using BankAppointments.Api.Services;
using MediatR;
namespace BankAppointments.Api.CQRS.Handlers;
public sealed class CreateAppointmentCommandHandler(IAppointmentCommandService commands) : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{ public Task<AppointmentResponse> Handle(CreateAppointmentCommand c, CancellationToken ct) => commands.CreateAsync(c.Request, ct); }
