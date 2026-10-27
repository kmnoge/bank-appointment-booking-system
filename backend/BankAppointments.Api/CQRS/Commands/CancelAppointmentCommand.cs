using MediatR;

namespace BankAppointments.Api.CQRS.Commands;

/// <summary>Write request: cancels an appointment without deleting its history.</summary>
public sealed record CancelAppointmentCommand(Guid AppointmentId) : IRequest<bool>;
