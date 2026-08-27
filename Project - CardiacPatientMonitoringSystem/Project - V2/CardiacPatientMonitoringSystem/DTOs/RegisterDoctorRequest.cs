namespace CardiacPatientMonitoringSystem.DTOs;

public class RegisterDoctorRequest
{
    // Identity account information
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    // Doctor information
    public int DepartmentId { get; set; }

    public int? SupervisorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
}