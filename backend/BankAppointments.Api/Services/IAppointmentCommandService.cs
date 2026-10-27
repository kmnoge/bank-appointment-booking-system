using BankAppointments.Api.DTOs;
namespace BankAppointments.Api.Services;

/// <summary>Write-side application boundary. Implementations may mutate persistent state.</summary>
public interface IAppointmentCommandService
{
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);
    Task<bool> CancelAsync(Guid appointmentId, CancellationToken cancellationToken);
}
