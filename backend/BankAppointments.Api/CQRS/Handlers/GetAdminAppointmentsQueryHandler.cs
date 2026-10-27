using BankAppointments.Api.CQRS.Queries;
using BankAppointments.Api.DTOs;
using BankAppointments.Api.Services;
using MediatR;
namespace BankAppointments.Api.CQRS.Handlers;
public sealed class GetAdminAppointmentsQueryHandler(IAppointmentQueryService queries) : IRequestHandler<GetAdminAppointmentsQuery, IReadOnlyList<AdminAppointmentResponse>>
{ public Task<IReadOnlyList<AdminAppointmentResponse>> Handle(GetAdminAppointmentsQuery q, CancellationToken ct) => queries.GetAdminAppointmentsAsync(q.BranchId, q.Date, q.Status, q.Search, ct); }
