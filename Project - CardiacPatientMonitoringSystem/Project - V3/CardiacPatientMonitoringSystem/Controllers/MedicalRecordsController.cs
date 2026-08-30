using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

    // Gets the authenticated Identity UserId from the JWT token.
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // Checks whether the current Doctor is connected
    // to the specified Patient through an Appointment
    // or a Medical Record.
    private async Task<bool> DoctorHasAccessToPatientAsync(
        int patientId,
        string currentUserId)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == currentUserId);

        if (doctor == null)
        {
            return false;
        }

        return await _context.Patients.AnyAsync(p =>
            p.Id == patientId &&
            (
                p.Appointments.Any(a => a.DoctorId == doctor.Id)
                ||
                p.MedicalRecords.Any(m => m.DoctorId == doctor.Id)
            )
        );
    }

    // GET: api/medicalrecords
    //
    // Admin:
    //     Can see all medical records.
    //
    // Doctor:
    //     Can see only medical records belonging to patients
    //     connected to that doctor.
    //
    // Patient:
    //     Can see only their own medical records.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var query = _context.MedicalRecords
            .AsQueryable();

        // Patient can only see their own records.
        if (User.IsInRole("Patient"))
        {
            query = query.Where(
                m => m.Patient.UserId == currentUserId
            );
        }

        // Doctor can only see records belonging to
        // patients connected to that doctor.
        else if (User.IsInRole("Doctor"))
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.UserId == currentUserId
                );

            if (doctor == null)
            {
                return Forbid();
            }

            query = query.Where(m =>
                m.Patient.Appointments.Any(
                    a => a.DoctorId == doctor.Id
                )
                ||
                m.Patient.MedicalRecords.Any(
                    r => r.DoctorId == doctor.Id
                )
            );
        }

        // Admin can see all records.
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Projection prevents circular JSON references.
        var records = await query
            .Select(m => new
            {
                m.Id,
                m.PatientId,
                PatientName = m.Patient.FullName,
                m.DoctorId,
                DoctorName = m.Doctor.FullName,
                m.Diagnosis,
                m.Notes,
                m.CreatedAt
            })
            .ToListAsync();

        return Ok(records);
    }

    // GET: api/medicalrecords/{id}
    //
    // Admin:
    //     Can access any medical record.
    //
    // Doctor:
    //     Can access records belonging to their patients.
    //
    // Patient:
    //     Can access only their own records.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var record = await _context.MedicalRecords
            .Where(m => m.Id == id)
            .Select(m => new
            {
                m.Id,
                m.PatientId,
                PatientUserId = m.Patient.UserId,
                PatientName = m.Patient.FullName,
                m.DoctorId,
                DoctorName = m.Doctor.FullName,
                m.Diagnosis,
                m.Notes,
                m.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (record == null)
        {
            return NotFound(new
            {
                message =
                    $"Medical record with ID {id} was not found."
            });
        }

        // Admin can access any record.
        if (User.IsInRole("Admin"))
        {
            return Ok(new
            {
                record.Id,
                record.PatientId,
                record.PatientName,
                record.DoctorId,
                record.DoctorName,
                record.Diagnosis,
                record.Notes,
                record.CreatedAt
            });
        }

        // Patient can access only their own record.
        if (User.IsInRole("Patient"))
        {
            if (record.PatientUserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(new
            {
                record.Id,
                record.PatientId,
                record.PatientName,
                record.DoctorId,
                record.DoctorName,
                record.Diagnosis,
                record.Notes,
                record.CreatedAt
            });
        }

        // Doctor can access only records of connected patients.
        if (User.IsInRole("Doctor"))
        {
            var hasAccess =
                await DoctorHasAccessToPatientAsync(
                    record.PatientId,
                    currentUserId
                );

            if (!hasAccess)
            {
                return Forbid();
            }

            return Ok(new
            {
                record.Id,
                record.PatientId,
                record.PatientName,
                record.DoctorId,
                record.DoctorName,
                record.Diagnosis,
                record.Notes,
                record.CreatedAt
            });
        }

        return Forbid();
    }

    // POST: api/medicalrecords
    //
    // Only Admin and Doctor can create medical records.
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create(
        CreateMedicalRecordRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
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

        // Doctor can only create a record for a patient
        // connected to them.
        if (User.IsInRole("Doctor"))
        {
            var hasAccess =
                await DoctorHasAccessToPatientAsync(
                    request.PatientId,
                    currentUserId
                );

            if (!hasAccess)
            {
                return Forbid();
            }

            // Doctor identity must match the authenticated doctor.
            var doctor =
                await _context.Doctors
                    .FirstOrDefaultAsync(
                        d => d.UserId == currentUserId
                    );

            if (doctor == null ||
                request.DoctorId != doctor.Id)
            {
                return Forbid();
            }
        }

        var record = new Models.MedicalRecord
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
            new
            {
                record.Id,
                record.PatientId,
                record.DoctorId,
                record.Diagnosis,
                record.Notes,
                record.CreatedAt
            }
        );
    }

    // PUT: api/medicalrecords/{id}
    //
    // Admin can update any medical record.
    //
    // Doctor can update only records belonging to
    // patients connected to them.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(
        int id,
        UpdateMedicalRecordRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

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

        // Doctor must have access to both the existing
        // and requested patient.
        if (User.IsInRole("Doctor"))
        {
            var hasAccessToCurrentPatient =
                await DoctorHasAccessToPatientAsync(
                    record.PatientId,
                    currentUserId
                );

            var hasAccessToRequestedPatient =
                await DoctorHasAccessToPatientAsync(
                    request.PatientId,
                    currentUserId
                );

            var doctor =
                await _context.Doctors
                    .FirstOrDefaultAsync(
                        d => d.UserId == currentUserId
                    );

            if (
                !hasAccessToCurrentPatient ||
                !hasAccessToRequestedPatient ||
                doctor == null ||
                request.DoctorId != doctor.Id
            )
            {
                return Forbid();
            }
        }

        record.PatientId = request.PatientId;
        record.DoctorId = request.DoctorId;
        record.Diagnosis = request.Diagnosis;
        record.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/medicalrecords/{id}
    //
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