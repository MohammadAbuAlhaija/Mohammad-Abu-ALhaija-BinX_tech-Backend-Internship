using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicalRecordsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/medicalrecords
    // Admin, Doctor and Patient can view medical records.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var records = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .ToListAsync();

        return Ok(records);
    }

    // GET: api/medicalrecords/{id}
    // Admin, Doctor and Patient can view a medical record.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (record == null)
        {
            return NotFound(new
            {
                message =
                    $"Medical record with ID {id} was not found."
            });
        }

        return Ok(record);
    }

    // POST: api/medicalrecords
    // Only Admin and Doctor can create medical records.
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create(
        CreateMedicalRecordRequest request)
    {
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {request.PatientId} was not found."
            });
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId);

        if (!doctorExists)
        {
            return NotFound(new
            {
                message =
                    $"Doctor with ID {request.DoctorId} was not found."
            });
        }

        var record = new MedicalRecord
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            Diagnosis = request.Diagnosis,
            Notes = request.Notes,
            CreatedAt = DateTime.Now
        };

        _context.MedicalRecords.Add(record);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = record.Id },
            record
        );
    }

    // PUT: api/medicalrecords/{id}
    // Only Admin and Doctor can update medical records.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(
        int id,
        UpdateMedicalRecordRequest request)
    {
        var record = await _context.MedicalRecords
            .FindAsync(id);

        if (record == null)
        {
            return NotFound(new
            {
                message =
                    $"Medical record with ID {id} was not found."
            });
        }

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {request.PatientId} was not found."
            });
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId);

        if (!doctorExists)
        {
            return NotFound(new
            {
                message =
                    $"Doctor with ID {request.DoctorId} was not found."
            });
        }

        record.PatientId = request.PatientId;
        record.DoctorId = request.DoctorId;
        record.Diagnosis = request.Diagnosis;
        record.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/medicalrecords/{id}
    // Only Admin can delete medical records.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.MedicalRecords
            .FindAsync(id);

        if (record == null)
        {
            return NotFound(new
            {
                message =
                    $"Medical record with ID {id} was not found."
            });
        }

        _context.MedicalRecords.Remove(record);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}