using BankAppointments.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BankAppointments.Api.Data;
public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<BankBranch> BankBranches => Set<BankBranch>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankBranch>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.Code).IsUnique(); e.Property(x => x.Code).HasMaxLength(20); e.Property(x => x.Name).HasMaxLength(120); e.Property(x => x.Address).HasMaxLength(250); });
        modelBuilder.Entity<Appointment>(e => {
            e.HasKey(x => x.Id); e.HasIndex(x => x.Reference).IsUnique(); e.HasIndex(x => new { x.BankBranchId, x.AppointmentAtUtc }).IsUnique().HasFilter("[Status] = 1");
            e.HasIndex(x => new { x.Email, x.CreatedAtUtc });
            e.Property(x => x.Reference).HasMaxLength(20); e.Property(x => x.FullName).HasMaxLength(120); e.Property(x => x.Email).HasMaxLength(254); e.Property(x => x.PhoneNumber).HasMaxLength(30); e.Property(x => x.ServiceType).HasMaxLength(80);
            e.HasOne(x => x.BankBranch).WithMany().HasForeignKey(x => x.BankBranchId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Status).HasConversion<int>();
        });
    }
}
