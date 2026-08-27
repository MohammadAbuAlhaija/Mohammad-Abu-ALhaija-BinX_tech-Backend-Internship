namespace CardiacPatientMonitoringSystem.DTOs;

public class UpdateMedicalRecordRequest
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string? Notes { get; set; }
}