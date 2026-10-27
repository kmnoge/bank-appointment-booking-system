using BankAppointments.Api.CQRS.Commands;
using BankAppointments.Api.CQRS.Validators;
using BankAppointments.Api.DTOs;
using Xunit;
namespace BankAppointments.UnitTests;
public sealed class CreateAppointmentCommandValidatorTests
{
    private readonly CreateAppointmentCommandValidator _validator = new();
    private static CreateAppointmentCommand ValidCommand() => new(new CreateAppointmentRequest
    {
        FullName = "Alex Mokoena", Email = "alex@example.com", PhoneNumber = "+27821234567",
        BankBranchId = 1, ServiceType = "Account services", AppointmentAt = new DateTime(2027, 1, 4, 9, 0, 0),
        IdempotencyKey = "test-idempotency-key-001"
    });

    [Fact]
    public void Valid_request_passes_validation() => Assert.True(_validator.Validate(ValidCommand()).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_email_fails_validation(string email)
    {
        var command = ValidCommand() with { Request = new CreateAppointmentRequest
        {
            FullName = "Alex Mokoena", Email = email, PhoneNumber = "+27821234567", BankBranchId = 1,
            ServiceType = "Account services", AppointmentAt = new DateTime(2027, 1, 4, 9, 0, 0), IdempotencyKey = "key-001"
        }};
        Assert.False(_validator.Validate(command).IsValid);
    }

    [Fact]
    public void Missing_idempotency_key_fails_validation()
    {
        var valid = ValidCommand().Request;
        var command = new CreateAppointmentCommand(new CreateAppointmentRequest
        {
            FullName = valid.FullName, Email = valid.Email, PhoneNumber = valid.PhoneNumber, BankBranchId = valid.BankBranchId,
            ServiceType = valid.ServiceType, AppointmentAt = valid.AppointmentAt, IdempotencyKey = ""
        });
        Assert.False(_validator.Validate(command).IsValid);
    }
}
