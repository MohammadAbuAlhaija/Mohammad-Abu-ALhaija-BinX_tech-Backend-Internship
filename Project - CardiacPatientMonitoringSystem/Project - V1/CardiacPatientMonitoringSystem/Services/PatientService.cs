using CardiacPatientMonitoringSystem.Repositories;

namespace CardiacPatientMonitoringSystem.Services;

public class PatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
    {
        int age = referenceDate.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > referenceDate.AddYears(-age).Date)
        {
            age--;
        }

        return age;
    }

    public async Task<string> GetPatientNameAsync(int id)
    {
    try
    {
        var patient = await _patientRepository.GetByIdAsync(id);

        if (patient == null)
        {
            return "Patient not found";
        }

        return patient.FullName;
    }
    catch (Exception)
    {
        return "Unable to retrieve patient";
    }
    }
}