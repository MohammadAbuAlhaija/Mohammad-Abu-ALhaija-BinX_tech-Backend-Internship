namespace CardiacPatientMonitoringSystem.DTOs;

public class CreatePatientMedicationRequest
{
    public int PatientId { get; set; }

    public int MedicationId { get; set; }

    public string Dosage { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}