using BankAppointments.Api.CQRS.Queries;
using BankAppointments.Api.Models;
using BankAppointments.Api.Services;
using MediatR;
namespace BankAppointments.Api.CQRS.Handlers;
public sealed class GetBranchesQueryHandler(IAppointmentQueryService queries) : IRequestHandler<GetBranchesQuery, IReadOnlyList<BankBranch>>
{ public Task<IReadOnlyList<BankBranch>> Handle(GetBranchesQuery query, CancellationToken cancellationToken) => queries.GetBranchesAsync(cancellationToken); }
