namespace CardiacPatientMonitoringSystem.DTOs;

public class CreateVitalSignRequest
{
    public int PatientId { get; set; }

    public int HeartRate { get; set; }

    public int SystolicBloodPressure { get; set; }

    public int DiastolicBloodPressure { get; set; }

    public DateTime MeasuredAt { get; set; }
}