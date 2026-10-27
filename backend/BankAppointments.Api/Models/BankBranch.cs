namespace BankAppointments.Api.Models;
public sealed class BankBranch
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public bool IsActive { get; set; } = true;
}
