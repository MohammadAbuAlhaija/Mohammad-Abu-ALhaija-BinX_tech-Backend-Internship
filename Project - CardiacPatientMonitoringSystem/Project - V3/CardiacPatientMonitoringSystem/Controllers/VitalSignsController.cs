using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VitalSignsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VitalSignsController(AppDbContext context)
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
    // or a MedicalRecord.
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
                p.Appointments.Any(
                    a => a.DoctorId == doctor.Id
                )
                ||
                p.MedicalRecords.Any(
                    m => m.DoctorId == doctor.Id
                )
            )
        );
    }

    // GET: api/vitalsigns
    //
    // Admin:
    //     Can see all Vital Signs.
    //
    // Doctor:
    //     Can see Vital Signs belonging to their patients.
    //
    // Patient:
    //     Can only see their own Vital Signs.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var query = _context.VitalSigns.AsQueryable();

        if (User.IsInRole("Patient"))
        {
            query = query.Where(v =>
                v.Patient.UserId == currentUserId
            );
        }
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

            query = query.Where(v =>
                v.Patient.Appointments.Any(
                    a => a.DoctorId == doctor.Id
                )
                ||
                v.Patient.MedicalRecords.Any(
                    m => m.DoctorId == doctor.Id
                )
            );
        }
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var vitalSigns = await query
            .Select(v => new
            {
                v.Id,
                v.PatientId,
                v.HeartRate,
                v.SystolicBloodPressure,
                v.DiastolicBloodPressure,
                v.MeasuredAt
            })
            .ToListAsync();

        return Ok(vitalSigns);
    }

    // GET: api/vitalsigns/1
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var vitalSign = await _context.VitalSigns
            .Where(v => v.Id == id)
            .Select(v => new
            {
                v.Id,
                v.PatientId,
                v.HeartRate,
                v.SystolicBloodPressure,
                v.DiastolicBloodPressure,
                v.MeasuredAt
            })
            .FirstOrDefaultAsync();

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message = $"Vital sign with ID {id} was not found."
            });
        }

        // Admin can access any Vital Sign.
        if (User.IsInRole("Admin"))
        {
            return Ok(vitalSign);
        }

        // Patient can only access their own Vital Sign.
        if (User.IsInRole("Patient"))
        {
            var ownsVitalSign =
                await _context.VitalSigns.AnyAsync(v =>
                    v.Id == id &&
                    v.Patient.UserId == currentUserId
                );

            if (!ownsVitalSign)
            {
                return Forbid();
            }

            return Ok(vitalSign);
        }

        // Doctor can only access Vital Signs
        // for patients connected to them.
        if (User.IsInRole("Doctor"))
        {
            var hasAccess =
                await DoctorHasAccessToPatientAsync(
                    vitalSign.PatientId,
                    currentUserId
                );

            if (!hasAccess)
            {
                return Forbid();
            }

            return Ok(vitalSign);
        }

        return Forbid();
    }

    // POST: api/vitalsigns
    //
    // Only Doctor and Admin can create Vital Signs.
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create(
        CreateVitalSignRequest request)
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

        // Doctor can only create Vital Signs
        // for a Patient connected to them.
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
        }

        var vitalSign = new VitalSign
        {
            PatientId = request.PatientId,
            HeartRate = request.HeartRate,
            SystolicBloodPressure =
                request.SystolicBloodPressure,
            DiastolicBloodPressure =
                request.DiastolicBloodPressure,
            MeasuredAt = request.MeasuredAt
        };

        _context.VitalSigns.Add(vitalSign);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = vitalSign.Id },
            new
            {
                vitalSign.Id,
                vitalSign.PatientId,
                vitalSign.HeartRate,
                vitalSign.SystolicBloodPressure,
                vitalSign.DiastolicBloodPressure,
                vitalSign.MeasuredAt
            }
        );
    }

    // PUT: api/vitalsigns/1
    //
    // Only Doctor and Admin can update Vital Signs.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(
        int id,
        UpdateVitalSignRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var vitalSign =
            await _context.VitalSigns.FindAsync(id);

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message =
                    $"Vital sign with ID {id} was not found."
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

        // Doctor must have access to both:
        // the current Patient and the requested Patient.
        if (User.IsInRole("Doctor"))
        {
            var hasAccessToCurrentPatient =
                await DoctorHasAccessToPatientAsync(
                    vitalSign.PatientId,
                    currentUserId
                );

            var hasAccessToRequestedPatient =
                await DoctorHasAccessToPatientAsync(
                    request.PatientId,
                    currentUserId
                );

            if (
                !hasAccessToCurrentPatient ||
                !hasAccessToRequestedPatient
            )
            {
                return Forbid();
            }
        }

        vitalSign.PatientId = request.PatientId;
        vitalSign.HeartRate = request.HeartRate;
        vitalSign.SystolicBloodPressure =
            request.SystolicBloodPressure;
        vitalSign.DiastolicBloodPressure =
            request.DiastolicBloodPressure;
        vitalSign.MeasuredAt = request.MeasuredAt;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/vitalsigns/1
    //
    // Only Doctor and Admin can delete Vital Signs.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var vitalSign =
            await _context.VitalSigns.FindAsync(id);

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message =
                    $"Vital sign with ID {id} was not found."
            });
        }

        if (User.IsInRole("Doctor"))
        {
            var hasAccess =
                await DoctorHasAccessToPatientAsync(
                    vitalSign.PatientId,
                    currentUserId
                );

            if (!hasAccess)
            {
                return Forbid();
            }
        }

        _context.VitalSigns.Remove(vitalSign);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}