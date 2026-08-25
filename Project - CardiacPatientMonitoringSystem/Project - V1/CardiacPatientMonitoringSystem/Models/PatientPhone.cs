namespace CardiacPatientMonitoringSystem.Models;

public class PatientPhone
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Type { get; set; }

    public Patient Patient { get; set; } = null!;
}