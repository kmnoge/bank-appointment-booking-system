using BankAppointments.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace BankAppointments.Api.Data.Migrations;

[DbContext(typeof(BookingDbContext))]
[Migration("202610150001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "BankBranches", columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_BankBranches", x => x.Id));
        migrationBuilder.CreateTable(name: "Appointments", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
            PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            BankBranchId = table.Column<int>(type: "int", nullable: false),
            ServiceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            AppointmentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            Status = table.Column<int>(type: "int", nullable: false),
            CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_Appointments", x => x.Id); table.ForeignKey("FK_Appointments_BankBranches_BankBranchId", x => x.BankBranchId, "BankBranches", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateIndex(name: "IX_BankBranches_Code", table: "BankBranches", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Appointments_Reference", table: "Appointments", column: "Reference", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Appointments_BankBranchId_AppointmentAtUtc", table: "Appointments", columns: new[] { "BankBranchId", "AppointmentAtUtc" }, unique: true, filter: "[Status] = 1");
        migrationBuilder.CreateIndex(name: "IX_Appointments_Email_CreatedAtUtc", table: "Appointments", columns: new[] { "Email", "CreatedAtUtc" });
    }
    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable(name: "Appointments"); migrationBuilder.DropTable(name: "BankBranches"); }
}
