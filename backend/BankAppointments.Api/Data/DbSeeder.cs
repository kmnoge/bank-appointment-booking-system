using BankAppointments.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BankAppointments.Api.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(BookingDbContext db)
    {
        if (await db.BankBranches.AnyAsync()) return;
        db.BankBranches.AddRange(
            new BankBranch { Code = "JHB-CBD", Name = "Johannesburg CBD Branch", Address = "Johannesburg CBD, Gauteng" },
            new BankBranch { Code = "BENONI", Name = "Benoni Branch", Address = "Benoni, Gauteng" },
            new BankBranch { Code = "SANDTON", Name = "Sandton Branch", Address = "Sandton, Gauteng" });
        await db.SaveChangesAsync();
    }
}
