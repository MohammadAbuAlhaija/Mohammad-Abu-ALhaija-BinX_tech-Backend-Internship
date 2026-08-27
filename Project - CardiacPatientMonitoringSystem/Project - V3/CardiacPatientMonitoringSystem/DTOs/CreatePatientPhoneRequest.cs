namespace CardiacPatientMonitoringSystem.DTOs;

public class CreatePatientPhoneRequest
{
    public int PatientId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Type { get; set; }
}