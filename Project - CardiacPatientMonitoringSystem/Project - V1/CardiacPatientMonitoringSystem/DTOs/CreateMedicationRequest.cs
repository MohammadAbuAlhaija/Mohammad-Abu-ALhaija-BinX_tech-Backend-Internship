namespace CardiacPatientMonitoringSystem.DTOs;

public class CreateMedicationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}