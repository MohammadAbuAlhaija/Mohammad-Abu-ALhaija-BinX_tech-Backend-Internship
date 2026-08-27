namespace CardiacPatientMonitoringSystem.DTOs;

public class RegisterAdminRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}