namespace BankAppointments.Api.Models;
public enum AppointmentStatus { Confirmed = 1, Cancelled = 2 }
public sealed class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Reference { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public int BankBranchId { get; set; }
    public BankBranch? BankBranch { get; set; }
    public required string ServiceType { get; set; }
    public DateTime AppointmentAtUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Confirmed;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
