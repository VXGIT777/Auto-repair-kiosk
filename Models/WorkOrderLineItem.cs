using System.ComponentModel.DataAnnotations;

namespace AutoRepairKiosk.Models;

public sealed class WorkOrderLineItem
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    [Required, MaxLength(160)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 9999)]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 999999)]
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}
