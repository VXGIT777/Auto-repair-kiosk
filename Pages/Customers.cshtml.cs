using System.ComponentModel.DataAnnotations;
using AutoRepairKiosk.Data;
using AutoRepairKiosk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Pages;

public sealed class CustomersModel(AppDbContext dbContext) : PageModel
{
    public IReadOnlyList<Customer> Customers { get; private set; } = [];

    [BindProperty]
    public CustomerInput Input { get; set; } = new();

    public async Task OnGetAsync(int? editId)
    {
        await LoadCustomersAsync();

        if (editId is not null)
        {
            var customer = Customers.SingleOrDefault(item => item.Id == editId.Value);
            if (customer is not null)
            {
                Input = CustomerInput.FromCustomer(customer);
            }
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCustomersAsync();
            return Page();
        }

        var customer = Input.Id is null
            ? new Customer()
            : await dbContext.Customers.FindAsync(Input.Id.Value);

        if (customer is null)
        {
            return NotFound();
        }

        customer.FirstName = Input.FirstName.Trim();
        customer.LastName = Input.LastName.Trim();
        customer.Phone = Input.Phone?.Trim();
        customer.Email = Input.Email?.Trim();
        customer.Notes = Input.Notes?.Trim();

        if (Input.Id is null)
        {
            dbContext.Customers.Add(customer);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Customers");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var customer = await dbContext.Customers.FindAsync(id);
        if (customer is not null)
        {
            dbContext.Customers.Remove(customer);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage("/Customers");
    }

    private async Task LoadCustomersAsync()
    {
        Customers = await dbContext.Customers
            .AsNoTracking()
            .Include(customer => customer.Vehicles)
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ToListAsync();
    }

    public sealed class CustomerInput
    {
        public int? Id { get; set; }

        [Required, Display(Name = "First name"), MaxLength(80)]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Last name"), MaxLength(80)]
        public string LastName { get; set; } = string.Empty;

        [Phone, MaxLength(30)]
        public string? Phone { get; set; }

        [EmailAddress, MaxLength(160)]
        public string? Email { get; set; }

        [MaxLength(240)]
        public string? Notes { get; set; }

        public static CustomerInput FromCustomer(Customer customer) => new()
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Email = customer.Email,
            Notes = customer.Notes
        };
    }
}
