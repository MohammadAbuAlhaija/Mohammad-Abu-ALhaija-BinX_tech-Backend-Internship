namespace CardiacPatientMonitoringSystem.DTOs;

public class UpdateAppointmentRequest
{
    public int PatientId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}