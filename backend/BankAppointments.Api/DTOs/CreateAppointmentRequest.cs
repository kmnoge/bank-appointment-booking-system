using System.ComponentModel.DataAnnotations;
namespace BankAppointments.Api.DTOs;
public sealed class CreateAppointmentRequest
{
    [Required, StringLength(120, MinimumLength = 2)] public string FullName { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = string.Empty;
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; init; } = string.Empty;
    [Range(1, int.MaxValue)] public int BankBranchId { get; init; }
    [Required, StringLength(80)] public string ServiceType { get; init; } = string.Empty;
    // ISO-8601 local appointment time; API interprets this as South African time (UTC+02:00).
    public DateTime AppointmentAt { get; init; }
    [Required, StringLength(100)] public string IdempotencyKey { get; init; } = string.Empty;
}
