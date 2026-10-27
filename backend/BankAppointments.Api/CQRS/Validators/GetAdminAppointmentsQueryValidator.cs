using BankAppointments.Api.CQRS.Queries;
using FluentValidation;
namespace BankAppointments.Api.CQRS.Validators;
public sealed class GetAdminAppointmentsQueryValidator : AbstractValidator<GetAdminAppointmentsQuery>
{
    public GetAdminAppointmentsQueryValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);
        RuleFor(x => x.Search).MaximumLength(100).When(x => x.Search is not null);
        RuleFor(x => x.Status).Must(s => string.IsNullOrWhiteSpace(s) || Enum.TryParse<BankAppointments.Api.Models.AppointmentStatus>(s, true, out _))
            .WithMessage("Status must be Confirmed or Cancelled.");
    }
}
