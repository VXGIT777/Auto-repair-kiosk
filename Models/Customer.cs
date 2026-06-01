using System.ComponentModel.DataAnnotations;

namespace AutoRepairKiosk.Models;

public sealed class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Phone, MaxLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, MaxLength(160)]
    public string? Email { get; set; }

    [MaxLength(240)]
    public string? Notes { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
