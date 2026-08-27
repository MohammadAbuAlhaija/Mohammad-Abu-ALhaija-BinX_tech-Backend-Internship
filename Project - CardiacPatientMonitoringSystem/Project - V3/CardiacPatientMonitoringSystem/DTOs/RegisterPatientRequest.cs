namespace CardiacPatientMonitoringSystem.DTOs;

public class RegisterPatientRequest
{
    // Identity account information
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    // Patient information
    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;
}