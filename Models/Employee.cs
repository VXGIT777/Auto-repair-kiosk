using System.ComponentModel.DataAnnotations;

namespace AutoRepairKiosk.Models;

public sealed class Employee
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string DisplayName { get; set; } = string.Empty;
}
