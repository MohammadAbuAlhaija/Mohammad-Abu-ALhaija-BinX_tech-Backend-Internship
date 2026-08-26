using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Services;

public class PatientVisitService
{
    private readonly AppDbContext _context;

    public PatientVisitService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> CreateVisitAsync(
        CreatePatientVisitRequest request)
    {
        // Business rule 1: Patient must exist
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return (false, $"Patient with ID {request.PatientId} was not found.");
        }

        // Business rule 2: Doctor must exist
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId);

        if (!doctorExists)
        {
            return (false, $"Doctor with ID {request.DoctorId} was not found.");
        }

        // Business rule 3: Measurement cannot be in the future
        if (request.MeasuredAt > DateTime.Now)
        {
            return (false, "Measurement date cannot be in the future.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var medicalRecord = new MedicalRecord
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
                CreatedAt = DateTime.Now
            };

            var vitalSign = new VitalSign
            {
                PatientId = request.PatientId,
                HeartRate = request.HeartRate,
                SystolicBloodPressure = request.SystolicBloodPressure,
                DiastolicBloodPressure = request.DiastolicBloodPressure,
                MeasuredAt = request.MeasuredAt
            };

            _context.MedicalRecords.Add(medicalRecord);
            _context.VitalSigns.Add(vitalSign);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (true, "Patient visit created successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}