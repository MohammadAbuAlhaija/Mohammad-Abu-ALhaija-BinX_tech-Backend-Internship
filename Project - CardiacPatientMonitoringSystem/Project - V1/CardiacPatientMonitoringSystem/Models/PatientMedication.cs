namespace CardiacPatientMonitoringSystem.Models;

public class PatientMedication
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int MedicationId { get; set; }

    public string Dosage { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public Patient Patient { get; set; } = null!;

    public Medication Medication { get; set; } = null!;
}