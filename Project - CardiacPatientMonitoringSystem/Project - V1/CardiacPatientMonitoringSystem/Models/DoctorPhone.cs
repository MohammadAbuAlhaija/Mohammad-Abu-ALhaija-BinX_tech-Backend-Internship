namespace CardiacPatientMonitoringSystem.Models;

public class DoctorPhone
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Type { get; set; }

    public Doctor Doctor { get; set; } = null!;
}