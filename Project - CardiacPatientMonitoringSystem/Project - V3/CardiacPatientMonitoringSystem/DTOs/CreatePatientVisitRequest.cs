namespace CardiacPatientMonitoringSystem.DTOs;

public class CreatePatientVisitRequest
{
    public int PatientId { get; set; }

    // Admin can choose the Doctor.
    // For Doctor users, this value will be replaced
    // with the DoctorId linked to the authenticated account.
    public int DoctorId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int HeartRate { get; set; }

    public int SystolicBloodPressure { get; set; }

    public int DiastolicBloodPressure { get; set; }

    public DateTime MeasuredAt { get; set; }
}