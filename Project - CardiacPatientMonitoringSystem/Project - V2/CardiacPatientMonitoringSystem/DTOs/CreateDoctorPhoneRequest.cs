namespace CardiacPatientMonitoringSystem.DTOs;

public class CreateDoctorPhoneRequest
{
    public int DoctorId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Type { get; set; }
}