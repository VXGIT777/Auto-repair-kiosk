using System.ComponentModel.DataAnnotations;
using AutoRepairKiosk.Data;
using AutoRepairKiosk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Pages;

public sealed class WorkOrdersModel(AppDbContext dbContext) : PageModel
{
    private const int LineItemSlots = 5;

    public IReadOnlyList<WorkOrder> WorkOrders { get; private set; } = [];
    public SelectList VehicleOptions { get; private set; } = new(Enumerable.Empty<SelectListItem>());
    public SelectList StatusOptions { get; private set; } = new(Enumerable.Empty<SelectListItem>());

    [BindProperty]
    public WorkOrderInput Input { get; set; } = WorkOrderInput.Empty();

    public async Task OnGetAsync(int? editId, int? vehicleId)
    {
        await LoadAsync(vehicleId);

        if (editId is not null)
        {
            var workOrder = await dbContext.WorkOrders
                .AsNoTracking()
                .Include(order => order.LineItems.OrderBy(item => item.Id))
                .SingleOrDefaultAsync(order => order.Id == editId.Value);

            if (workOrder is not null)
            {
                Input = WorkOrderInput.FromWorkOrder(workOrder);
            }
        }
        else if (vehicleId is not null)
        {
            Input.VehicleId = vehicleId.Value;
        }

        Input.EnsureLineItemSlots(LineItemSlots);
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!await dbContext.Vehicles.AnyAsync(vehicle => vehicle.Id == Input.VehicleId))
        {
            ModelState.AddModelError("Input.VehicleId", "Select a valid vehicle.");
        }

        var populatedLineItems = Input.LineItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Description))
            .ToList();

        if (populatedLineItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one line item.");
        }

        if (!ModelState.IsValid)
        {
            Input.EnsureLineItemSlots(LineItemSlots);
            await LoadAsync(null);
            return Page();
        }

        var workOrder = Input.Id is null
            ? new WorkOrder()
            : await dbContext.WorkOrders
                .Include(order => order.LineItems)
                .SingleOrDefaultAsync(order => order.Id == Input.Id.Value);

        if (workOrder is null)
        {
            return NotFound();
        }

        workOrder.VehicleId = Input.VehicleId;
        workOrder.Complaint = Input.Complaint.Trim();
        workOrder.Status = Input.Status;
        workOrder.OpenedOn = Input.OpenedOn;
        workOrder.PromisedOn = Input.PromisedOn;
        workOrder.TechnicianNotes = Input.TechnicianNotes?.Trim();

        var retainedIds = populatedLineItems
            .Where(item => item.Id is not null)
            .Select(item => item.Id!.Value)
            .ToHashSet();

        foreach (var existing in workOrder.LineItems.Where(item => !retainedIds.Contains(item.Id)).ToList())
        {
            dbContext.WorkOrderLineItems.Remove(existing);
        }

        foreach (var lineInput in populatedLineItems)
        {
            var lineItem = lineInput.Id is null
                ? new WorkOrderLineItem()
                : workOrder.LineItems.SingleOrDefault(item => item.Id == lineInput.Id.Value) ?? new WorkOrderLineItem();

            lineItem.Description = lineInput.Description.Trim();
            lineItem.Quantity = lineInput.Quantity;
            lineItem.UnitPrice = lineInput.UnitPrice;

            if (lineInput.Id is null || lineItem.Id == 0)
            {
                workOrder.LineItems.Add(lineItem);
            }
        }

        if (Input.Id is null)
        {
            dbContext.WorkOrders.Add(workOrder);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/WorkOrders", new { vehicleId = Input.VehicleId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var workOrder = await dbContext.WorkOrders.FindAsync(id);
        if (workOrder is not null)
        {
            dbContext.WorkOrders.Remove(workOrder);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage("/WorkOrders");
    }

    private async Task LoadAsync(int? vehicleId)
    {
        var vehicles = await dbContext.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Customer)
            .OrderBy(vehicle => vehicle.Customer!.LastName)
            .ThenBy(vehicle => vehicle.Make)
            .Select(vehicle => new
            {
                vehicle.Id,
                Label = vehicle.Year + " " + vehicle.Make + " " + vehicle.Model + " - " +
                    vehicle.Customer!.FirstName + " " + vehicle.Customer.LastName
            })
            .ToListAsync();

        VehicleOptions = new SelectList(vehicles, "Id", "Label");
        StatusOptions = new SelectList(Enum.GetValues<WorkOrderStatus>().Select(status => new { Id = status, Name = status.ToString() }), "Id", "Name");

        var query = dbContext.WorkOrders
            .AsNoTracking()
            .Include(order => order.Vehicle)
            .ThenInclude(vehicle => vehicle!.Customer)
            .Include(order => order.LineItems)
            .OrderByDescending(order => order.OpenedOn)
            .ThenByDescending(order => order.Id)
            .AsQueryable();

        if (vehicleId is not null)
        {
            query = query.Where(order => order.VehicleId == vehicleId.Value);
        }

        WorkOrders = await query.ToListAsync();
    }

    public sealed class WorkOrderInput
    {
        public int? Id { get; set; }

        [Display(Name = "Vehicle"), Range(1, int.MaxValue, ErrorMessage = "Select a vehicle.")]
        public int VehicleId { get; set; }

        [Required, MaxLength(120)]
        public string Complaint { get; set; } = string.Empty;

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;

        [Display(Name = "Opened")]
        public DateOnly OpenedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Promised")]
        public DateOnly? PromisedOn { get; set; }

        [Display(Name = "Technician notes"), MaxLength(1000)]
        public string? TechnicianNotes { get; set; }

        public List<LineItemInput> LineItems { get; set; } = [];

        public void EnsureLineItemSlots(int slotCount)
        {
            while (LineItems.Count < slotCount)
            {
                LineItems.Add(new LineItemInput());
            }
        }

        public static WorkOrderInput Empty()
        {
            var input = new WorkOrderInput();
            input.EnsureLineItemSlots(WorkOrdersModel.LineItemSlots);
            return input;
        }

        public static WorkOrderInput FromWorkOrder(WorkOrder workOrder)
        {
            var input = new WorkOrderInput
            {
                Id = workOrder.Id,
                VehicleId = workOrder.VehicleId,
                Complaint = workOrder.Complaint,
                Status = workOrder.Status,
                OpenedOn = workOrder.OpenedOn,
                PromisedOn = workOrder.PromisedOn,
                TechnicianNotes = workOrder.TechnicianNotes,
                LineItems = workOrder.LineItems.Select(LineItemInput.FromLineItem).ToList()
            };

            input.EnsureLineItemSlots(LineItemSlots);
            return input;
        }
    }

    public sealed class LineItemInput
    {
        public int? Id { get; set; }

        [MaxLength(160)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 9999)]
        public decimal Quantity { get; set; } = 1;

        [Display(Name = "Unit price"), Range(0, 999999)]
        public decimal UnitPrice { get; set; }

        public static LineItemInput FromLineItem(WorkOrderLineItem item) => new()
        {
            Id = item.Id,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };
    }
}
