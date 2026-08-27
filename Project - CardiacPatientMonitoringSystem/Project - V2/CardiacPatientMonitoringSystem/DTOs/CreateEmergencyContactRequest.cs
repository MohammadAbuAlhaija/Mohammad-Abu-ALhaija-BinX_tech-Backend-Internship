namespace CardiacPatientMonitoringSystem.DTOs;

public class CreateEmergencyContactRequest
{
    public int PatientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;
}