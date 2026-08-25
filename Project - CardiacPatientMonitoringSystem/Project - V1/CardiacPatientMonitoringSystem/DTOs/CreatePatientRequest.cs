namespace CardiacPatientMonitoringSystem.DTOs;

public class CreatePatientRequest
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;
}