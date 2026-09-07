using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientMedicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientMedicationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/patientmedications
    // Admin and Doctor can view all patient medications.
    // Patient can view only their own medications.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var patientMedicationsQuery = _context.PatientMedications
            .AsNoTracking()
            .AsQueryable();

        // Patients can only view their own medications.
        if (User.IsInRole("Patient"))
        {
            var patientIdClaim =
                User.FindFirstValue("PatientId");

            if (!int.TryParse(patientIdClaim, out var patientId))
            {
                return Forbid();
            }

            patientMedicationsQuery =
                patientMedicationsQuery
                    .Where(pm => pm.PatientId == patientId);
        }

        var patientMedications =
            await patientMedicationsQuery
                .Select(pm => new
                {
                    pm.Id,
                    pm.PatientId,
                    PatientName = pm.Patient.FullName,
                    pm.MedicationId,
                    MedicationName = pm.Medication.Name,
                    MedicationDescription = pm.Medication.Description,
                    pm.Dosage,
                    pm.Frequency,
                    pm.StartDate,
                    pm.EndDate
                })
                .ToListAsync();

        return Ok(patientMedications);
    }

    // GET: api/patientmedications/{id}
    // Admin and Doctor can view any patient medication.
    // Patient can view only their own medication.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var patientMedication =
            await _context.PatientMedications
                .Include(pm => pm.Patient)
                .Include(pm => pm.Medication)
                .FirstOrDefaultAsync(pm => pm.Id == id);

        if (patientMedication == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient medication with ID {id} was not found."
            });
        }

        // Ownership check for Patient users.
        if (User.IsInRole("Patient"))
        {
            var patientIdClaim =
                User.FindFirstValue("PatientId");

            if (!int.TryParse(patientIdClaim, out var patientId))
            {
                return Forbid();
            }

            if (patientMedication.PatientId != patientId)
            {
                return Forbid();
            }
        }

        return Ok(patientMedication);
    }

    // POST: api/patientmedications
    // Only Admin can create patient medication records.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePatientMedicationRequest request)
    {
        var patientExists =
            await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {request.PatientId} was not found."
            });
        }

        var medicationExists =
            await _context.Medications
                .AnyAsync(m => m.Id == request.MedicationId);

        if (!medicationExists)
        {
            return NotFound(new
            {
                message =
                    $"Medication with ID {request.MedicationId} was not found."
            });
        }

        var patientMedication = new PatientMedication
        {
            PatientId = request.PatientId,
            MedicationId = request.MedicationId,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.PatientMedications.Add(patientMedication);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = patientMedication.Id },
            patientMedication
        );
    }

    // PUT: api/patientmedications/{id}
    // Only Admin can update patient medication records.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePatientMedicationRequest request)
    {
        var patientMedication =
            await _context.PatientMedications.FindAsync(id);

        if (patientMedication == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient medication with ID {id} was not found."
            });
        }

        var patientExists =
            await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {request.PatientId} was not found."
            });
        }

        var medicationExists =
            await _context.Medications
                .AnyAsync(m => m.Id == request.MedicationId);

        if (!medicationExists)
        {
            return NotFound(new
            {
                message =
                    $"Medication with ID {request.MedicationId} was not found."
            });
        }

        patientMedication.PatientId = request.PatientId;
        patientMedication.MedicationId = request.MedicationId;
        patientMedication.Dosage = request.Dosage;
        patientMedication.Frequency = request.Frequency;
        patientMedication.StartDate = request.StartDate;
        patientMedication.EndDate = request.EndDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/patientmedications/{id}
    // Only Admin can delete patient medication records.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var patientMedication =
            await _context.PatientMedications.FindAsync(id);

        if (patientMedication == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient medication with ID {id} was not found."
            });
        }

        _context.PatientMedications.Remove(patientMedication);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}