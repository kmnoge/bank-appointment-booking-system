namespace BankAppointments.Api.DTOs;

public sealed record AdminAppointmentResponse(
    Guid Id, string Reference, string FullName, string Email, string PhoneNumber,
    int BankBranchId, string Branch, string ServiceType, DateTime AppointmentAt,
    string Status, DateTime CreatedAt);
