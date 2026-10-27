using BankAppointments.Api.DTOs;
using MediatR;

namespace BankAppointments.Api.CQRS.Queries;

/// <summary>Read request: filters appointments for the branch administration dashboard.</summary>
public sealed record GetAdminAppointmentsQuery(int? BranchId, DateOnly? Date, string? Status, string? Search)
    : IRequest<IReadOnlyList<AdminAppointmentResponse>>;
