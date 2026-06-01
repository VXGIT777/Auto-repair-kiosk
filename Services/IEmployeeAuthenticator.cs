using AutoRepairKiosk.Models;

namespace AutoRepairKiosk.Services;

public interface IEmployeeAuthenticator
{
    Task<Employee?> AuthenticateAsync(string username, string password);
}
