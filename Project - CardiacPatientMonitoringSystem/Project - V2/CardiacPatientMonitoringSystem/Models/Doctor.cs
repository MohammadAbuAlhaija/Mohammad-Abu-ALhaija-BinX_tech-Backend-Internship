using Microsoft.AspNetCore.Identity;

namespace CardiacPatientMonitoringSystem.Models;

public class Doctor
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;
    
    public int DepartmentId { get; set; }

    public int? SupervisorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;

    public Doctor? Supervisor { get; set; }

    public ICollection<Doctor> Subordinates { get; set; } = new List<Doctor>();

    public ICollection<DoctorPhone> Phones { get; set; } = new List<DoctorPhone>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}