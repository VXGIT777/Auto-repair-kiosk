using System.ComponentModel.DataAnnotations;

namespace AutoRepairKiosk.Models;

public sealed class Vehicle
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required, MaxLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Model { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Color { get; set; }

    [MaxLength(40)]
    public string? LicensePlate { get; set; }

    [MaxLength(17), MinLength(11)]
    public string? Vin { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

    public string Label => $"{Year} {Make} {Model}".Trim();
}
