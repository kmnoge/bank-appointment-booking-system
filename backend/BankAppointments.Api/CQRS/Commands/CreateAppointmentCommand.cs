using BankAppointments.Api.DTOs;
using MediatR;

namespace BankAppointments.Api.CQRS.Commands;

/// <summary>Write request: creates a customer appointment.</summary>
public sealed record CreateAppointmentCommand(CreateAppointmentRequest Request) : IRequest<AppointmentResponse>;
