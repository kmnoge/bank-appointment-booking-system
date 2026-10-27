using BankAppointments.Api.Data;
using BankAppointments.Api.DTOs;
using BankAppointments.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BankAppointments.Api.Services;
public sealed class AppointmentQueryService(BookingDbContext db) : IAppointmentQueryService
{
    public async Task<IReadOnlyList<BankBranch>> GetBranchesAsync(CancellationToken cancellationToken) =>
        await db.BankBranches.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AdminAppointmentResponse>> GetAdminAppointmentsAsync(int? branchId, DateOnly? date, string? status, string? search, CancellationToken cancellationToken)
    {
        var query = db.Appointments.AsNoTracking().Include(x => x.BankBranch).AsQueryable();
        if (branchId.HasValue) query = query.Where(x => x.BankBranchId == branchId.Value);
        if (date.HasValue)
        {
            var startLocal = date.Value.ToDateTime(TimeOnly.MinValue);
            var startUtc = new DateTimeOffset(startLocal, TimeSpan.FromHours(2)).UtcDateTime;
            var endUtc = startUtc.AddDays(1);
            query = query.Where(x => x.AppointmentAtUtc >= startUtc && x.AppointmentAtUtc < endUtc);
        }
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Reference.Contains(term) || x.FullName.Contains(term) || x.Email.Contains(term) || x.PhoneNumber.Contains(term));
        }
        return await query.OrderBy(x => x.AppointmentAtUtc).Take(500)
            .Select(x => new AdminAppointmentResponse(x.Id, x.Reference, x.FullName, x.Email, x.PhoneNumber, x.BankBranchId, x.BankBranch!.Name, x.ServiceType, x.AppointmentAtUtc.AddHours(2), x.Status.ToString(), x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

}
