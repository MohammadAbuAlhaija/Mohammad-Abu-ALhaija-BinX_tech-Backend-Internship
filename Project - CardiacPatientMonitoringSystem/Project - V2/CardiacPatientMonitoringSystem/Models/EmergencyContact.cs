namespace CardiacPatientMonitoringSystem.Models;

public class EmergencyContact
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
}