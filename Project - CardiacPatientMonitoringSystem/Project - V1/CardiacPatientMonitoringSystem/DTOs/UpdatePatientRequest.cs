namespace CardiacPatientMonitoringSystem.DTOs;

public class UpdatePatientRequest
{
    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;
}