namespace CardiacPatientMonitoringSystem.Models;

public class Medication
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<PatientMedication> PatientMedications { get; set; }
        = new List<PatientMedication>();
}