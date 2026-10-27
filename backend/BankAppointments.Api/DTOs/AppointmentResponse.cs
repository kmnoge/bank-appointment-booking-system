namespace BankAppointments.Api.DTOs;
public sealed record AppointmentResponse(Guid Id, string Reference, string Message, string FullName, string Branch, string ServiceType, DateTime AppointmentAt, string Status);
