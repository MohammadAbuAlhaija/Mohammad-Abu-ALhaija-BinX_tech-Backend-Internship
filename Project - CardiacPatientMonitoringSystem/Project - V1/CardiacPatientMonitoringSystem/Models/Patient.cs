using Microsoft.AspNetCore.Identity;

namespace CardiacPatientMonitoringSystem.Models;

public class Patient
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public ICollection<PatientPhone> Phones { get; set; } = new List<PatientPhone>();

    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();

    public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();

    public ICollection<PatientMedication> PatientMedications { get; set; } = new List<PatientMedication>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}