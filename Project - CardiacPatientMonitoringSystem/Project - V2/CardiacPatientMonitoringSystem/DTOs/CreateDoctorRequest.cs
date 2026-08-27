namespace CardiacPatientMonitoringSystem.DTOs;

public class CreateDoctorRequest
{
    public string UserId { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public int? SupervisorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;
}