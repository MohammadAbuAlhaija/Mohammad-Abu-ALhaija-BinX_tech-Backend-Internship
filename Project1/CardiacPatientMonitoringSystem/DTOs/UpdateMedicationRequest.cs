namespace CardiacPatientMonitoringSystem.DTOs;

public class UpdateMedicationRequest
{
    public int PatientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Dosage { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}