using CardiacPatientMonitoringSystem.Models;

namespace CardiacPatientMonitoringSystem.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
}