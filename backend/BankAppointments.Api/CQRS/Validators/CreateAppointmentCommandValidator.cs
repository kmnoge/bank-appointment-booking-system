using BankAppointments.Api.CQRS.Commands;
using FluentValidation;

namespace BankAppointments.Api.CQRS.Validators;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.FullName).NotEmpty().MinimumLength(2).MaximumLength(120);
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254);
            RuleFor(x => x.Request.PhoneNumber).NotEmpty().MaximumLength(30);
            RuleFor(x => x.Request.BankBranchId).GreaterThan(0);
            RuleFor(x => x.Request.ServiceType).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Request.IdempotencyKey).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Request.AppointmentAt).Must(x => x != default).WithMessage("An appointment date and time is required.");
        });
    }
}
