using BankAppointments.Api.CQRS.Commands;
using FluentValidation;
namespace BankAppointments.Api.CQRS.Validators;
public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator() => RuleFor(x => x.AppointmentId).NotEmpty();
}
