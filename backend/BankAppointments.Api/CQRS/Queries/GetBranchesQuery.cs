using BankAppointments.Api.Models;
using MediatR;

namespace BankAppointments.Api.CQRS.Queries;

/// <summary>Read request: lists active bank branches.</summary>
public sealed record GetBranchesQuery : IRequest<IReadOnlyList<BankBranch>>;
