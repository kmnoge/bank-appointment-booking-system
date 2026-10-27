using System.Security.Cryptography;
using System.Text;
using BankAppointments.Api.Data;
using BankAppointments.Api.DTOs;
using BankAppointments.Api.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
namespace BankAppointments.Api.Services;
public sealed class AppointmentCommandService(BookingDbContext db, IConnectionMultiplexer redis, ILogger<AppointmentCommandService> logger) : IAppointmentCommandService
{
    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var localTime = DateTime.SpecifyKind(request.AppointmentAt, DateTimeKind.Unspecified);
        var southAfricanNow = DateTime.UtcNow.AddHours(2);
        if (localTime <= southAfricanNow.AddMinutes(30) || localTime > southAfricanNow.AddDays(90))
            throw new BookingValidationException("Appointments must be booked at least 30 minutes ahead and within the next 90 days.");
        if (localTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || localTime.TimeOfDay < TimeSpan.FromHours(9) || localTime.TimeOfDay >= TimeSpan.FromHours(16))
            throw new BookingValidationException("Appointments are available Monday to Friday, 09:00–16:00.");
        if (localTime.Minute % 30 != 0 || localTime.Second != 0)
            throw new BookingValidationException("Choose a 30-minute appointment slot (for example, 09:00 or 09:30).");
        var branch = await db.BankBranches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.BankBranchId && x.IsActive, cancellationToken)
            ?? throw new BookingValidationException("The selected branch is not available.");
        var redisDb = redis.GetDatabase();
        var slotKey = $"bank-appointments:slot:{request.BankBranchId}:{localTime:yyyyMMddHHmm}";
        var lockToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        // Redis distributed lock coordinates multiple API replicas. The unique SQL index remains the final correctness guard.
        var acquired = await redisDb.StringSetAsync(slotKey, lockToken, TimeSpan.FromSeconds(60), When.NotExists);
        if (!acquired) throw new SlotUnavailableException("That appointment slot is being booked or is already taken. Please choose another time.");
        try
        {
            var idempotencyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.IdempotencyKey)));
            var idempotencyReference = $"ID-{idempotencyHash[..16]}";
            var existing = await db.Appointments.AsNoTracking().Include(x => x.BankBranch).FirstOrDefaultAsync(x => x.Reference == idempotencyReference, cancellationToken);
            if (existing is not null) return ToResponse(existing, existing.BankBranch?.Name ?? branch.Name);
            var utcTime = new DateTimeOffset(localTime, TimeSpan.FromHours(2)).UtcDateTime;
            if (await db.Appointments.AnyAsync(x => x.BankBranchId == request.BankBranchId && x.AppointmentAtUtc == utcTime && x.Status == AppointmentStatus.Confirmed, cancellationToken))
                throw new SlotUnavailableException("That appointment slot is no longer available. Please choose another time.");
            var appointment = new Appointment { Reference = $"ID-{idempotencyHash[..16]}", FullName = request.FullName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), PhoneNumber = request.PhoneNumber.Trim(), BankBranchId = branch.Id, ServiceType = request.ServiceType.Trim(), AppointmentAtUtc = utcTime };
            db.Appointments.Add(appointment);
            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateException ex) { logger.LogInformation(ex, "Database rejected a concurrent booking for branch {BranchId} at {AppointmentAtUtc}.", branch.Id, utcTime); throw new SlotUnavailableException("That appointment slot was just booked. Please choose another time."); }
            return ToResponse(appointment, branch.Name);
        }
        finally
        {
            // Compare-and-delete prevents releasing a lock that expired and was acquired by another request.
            const string releaseScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
            await redisDb.ScriptEvaluateAsync(releaseScript, new RedisKey[] { slotKey }, new RedisValue[] { lockToken });
        }
    }
    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (appointment is null) return false;
        if (appointment.Status == AppointmentStatus.Cancelled) return true;
        appointment.Status = AppointmentStatus.Cancelled;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AppointmentResponse ToResponse(Appointment a, string branch) => new(a.Id, a.Reference, "Your appointment has been booked successfully.", a.FullName, branch, a.ServiceType, DateTime.SpecifyKind(a.AppointmentAtUtc.AddHours(2), DateTimeKind.Unspecified), a.Status.ToString());
}
