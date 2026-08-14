namespace CardiacPatientMonitoringSystem.DTOs;

public class UpdatePatientRequest
{
    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}