using System.ComponentModel.DataAnnotations;

namespace AutoRepairKiosk.Models;

public enum WorkOrderStatus
{
    Open = 0,
    InProgress = 1,
    WaitingForParts = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class WorkOrder
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    [Required, MaxLength(120)]
    public string Complaint { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? TechnicianNotes { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

    [DataType(DataType.Date)]
    public DateOnly OpenedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [DataType(DataType.Date)]
    public DateOnly? PromisedOn { get; set; }

    public ICollection<WorkOrderLineItem> LineItems { get; set; } = new List<WorkOrderLineItem>();

    public decimal Total => LineItems.Sum(item => item.LineTotal);
}
