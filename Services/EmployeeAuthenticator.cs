using AutoRepairKiosk.Data;
using AutoRepairKiosk.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairKiosk.Services;

public sealed class EmployeeAuthenticator(AppDbContext dbContext) : IEmployeeAuthenticator
{
    public Task<Employee?> AuthenticateAsync(string username, string password)
    {
        var normalizedUsername = username.Trim();

        return dbContext.Employees
            .AsNoTracking()
            .SingleOrDefaultAsync(employee =>
                employee.Username == normalizedUsername &&
                employee.Password == password);
    }
}
