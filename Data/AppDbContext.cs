using AutoRepairKiosk.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderLineItem> WorkOrderLineItems => Set<WorkOrderLineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasIndex(employee => employee.Username)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasMany(customer => customer.Vehicles)
            .WithOne(vehicle => vehicle.Customer)
            .HasForeignKey(vehicle => vehicle.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasMany(vehicle => vehicle.WorkOrders)
            .WithOne(workOrder => workOrder.Vehicle)
            .HasForeignKey(workOrder => workOrder.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrder>()
            .Property(workOrder => workOrder.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        modelBuilder.Entity<WorkOrder>()
            .HasMany(workOrder => workOrder.LineItems)
            .WithOne(item => item.WorkOrder)
            .HasForeignKey(item => item.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrderLineItem>()
            .Property(item => item.Quantity)
            .HasPrecision(10, 2);

        modelBuilder.Entity<WorkOrderLineItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(10, 2);
    }
}
