using BankAppointments.Api.DTOs;
using BankAppointments.Api.Models;
namespace BankAppointments.Api.Services;

/// <summary>Read-side application boundary. Implementations must not mutate persistent state.</summary>
public interface IAppointmentQueryService
{
    Task<IReadOnlyList<BankBranch>> GetBranchesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminAppointmentResponse>> GetAdminAppointmentsAsync(int? branchId, DateOnly? date, string? status, string? search, CancellationToken cancellationToken);
}
