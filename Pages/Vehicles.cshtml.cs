using System.ComponentModel.DataAnnotations;
using AutoRepairKiosk.Data;
using AutoRepairKiosk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Pages;

public sealed class VehiclesModel(AppDbContext dbContext) : PageModel
{
    public IReadOnlyList<Vehicle> Vehicles { get; private set; } = [];
    public SelectList CustomerOptions { get; private set; } = new(Enumerable.Empty<SelectListItem>());

    [BindProperty]
    public VehicleInput Input { get; set; } = new();

    public async Task OnGetAsync(int? editId, int? customerId)
    {
        await LoadAsync(customerId);

        if (editId is not null)
        {
            var vehicle = Vehicles.SingleOrDefault(item => item.Id == editId.Value)
                ?? await dbContext.Vehicles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == editId.Value);
            if (vehicle is not null)
            {
                Input = VehicleInput.FromVehicle(vehicle);
            }
        }
        else if (customerId is not null)
        {
            Input.CustomerId = customerId.Value;
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!await dbContext.Customers.AnyAsync(customer => customer.Id == Input.CustomerId))
        {
            ModelState.AddModelError("Input.CustomerId", "Select a valid customer.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(null);
            return Page();
        }

        var vehicle = Input.Id is null
            ? new Vehicle()
            : await dbContext.Vehicles.FindAsync(Input.Id.Value);

        if (vehicle is null)
        {
            return NotFound();
        }

        vehicle.CustomerId = Input.CustomerId;
        vehicle.Year = Input.Year;
        vehicle.Make = Input.Make.Trim();
        vehicle.Model = Input.Model.Trim();
        vehicle.Color = Input.Color?.Trim();
        vehicle.LicensePlate = Input.LicensePlate?.Trim();
        vehicle.Vin = Input.Vin?.Trim();

        if (Input.Id is null)
        {
            dbContext.Vehicles.Add(vehicle);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Vehicles", new { customerId = Input.CustomerId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var vehicle = await dbContext.Vehicles.FindAsync(id);
        if (vehicle is not null)
        {
            dbContext.Vehicles.Remove(vehicle);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage("/Vehicles");
    }

    private async Task LoadAsync(int? customerId)
    {
        var customerOptions = await dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .Select(customer => new
            {
                customer.Id,
                FullName = customer.FirstName + " " + customer.LastName
            })
            .ToListAsync();

        CustomerOptions = new SelectList(customerOptions, "Id", "FullName");

        var query = dbContext.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Customer)
            .OrderBy(vehicle => vehicle.Customer!.LastName)
            .ThenBy(vehicle => vehicle.Make)
            .ThenBy(vehicle => vehicle.Model)
            .AsQueryable();

        if (customerId is not null)
        {
            query = query.Where(vehicle => vehicle.CustomerId == customerId.Value);
        }

        Vehicles = await query.ToListAsync();
    }

    public sealed class VehicleInput
    {
        public int? Id { get; set; }

        [Display(Name = "Customer"), Range(1, int.MaxValue, ErrorMessage = "Select a customer.")]
        public int CustomerId { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; } = DateTime.Today.Year;

        [Required, MaxLength(80)]
        public string Make { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? Color { get; set; }

        [Display(Name = "Plate"), MaxLength(40)]
        public string? LicensePlate { get; set; }

        [Display(Name = "VIN"), MinLength(11), MaxLength(17)]
        public string? Vin { get; set; }

        public static VehicleInput FromVehicle(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            Year = vehicle.Year,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Color = vehicle.Color,
            LicensePlate = vehicle.LicensePlate,
            Vin = vehicle.Vin
        };
    }
}
