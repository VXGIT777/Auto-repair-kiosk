using AutoRepairKiosk.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Data;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public async Task InitializeAsync()
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Employees.AnyAsync())
        {
            dbContext.Employees.AddRange(
                new Employee { Username = "admin", Password = "password", DisplayName = "Shop Manager" },
                new Employee { Username = "tech", Password = "wrench", DisplayName = "Service Technician" });
        }

        if (!await dbContext.Customers.AnyAsync())
        {
            var customer = new Customer
            {
                FirstName = "Jordan",
                LastName = "Lee",
                Phone = "555-0134",
                Email = "jordan.lee@example.com",
                Notes = "Prefers text updates."
            };

            var vehicle = new Vehicle
            {
                Customer = customer,
                Year = 2018,
                Make = "Toyota",
                Model = "Camry",
                Color = "Silver",
                LicensePlate = "KSK-2048",
                Vin = "4T1B11HK8JU000001"
            };

            dbContext.WorkOrders.Add(new WorkOrder
            {
                Vehicle = vehicle,
                Complaint = "Brake pedal feels soft",
                Status = WorkOrderStatus.InProgress,
                OpenedOn = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PromisedOn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                TechnicianNotes = "Inspect hydraulics and front pads.",
                LineItems =
                {
                    new WorkOrderLineItem { Description = "Brake inspection", Quantity = 1, UnitPrice = 89.95m },
                    new WorkOrderLineItem { Description = "DOT 3 brake fluid", Quantity = 1, UnitPrice = 14.50m }
                }
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
