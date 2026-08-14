namespace CardiacPatientMonitoringSystem.Models;

public class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public Patient Patient { get; set; } = null!;
}