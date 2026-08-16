namespace CardiacPatientMonitoringSystem.Services;

public class PatientService
{
    public int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
    {
        int age = referenceDate.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > referenceDate.AddYears(-age).Date)
        {
            age--;
        }

        return age;
    }
}