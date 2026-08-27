using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // Gets the authenticated Identity UserId from the JWT.
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // Gets the Patient record linked to the current Identity user.
    private async Task<Patient?> GetCurrentPatientAsync(
        string currentUserId)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(
                p => p.UserId == currentUserId
            );
    }

    // Gets the Doctor record linked to the current Identity user.
    private async Task<Doctor?> GetCurrentDoctorAsync(
        string currentUserId)
    {
        return await _context.Doctors
            .FirstOrDefaultAsync(
                d => d.UserId == currentUserId
            );
    }

    // GET: api/appointments
    //
    // Patient:
    //     Sees only their own appointments.
    //
    // Doctor:
    //     Sees only appointments assigned to them.
    //
    // Admin:
    //     Sees all appointments.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll(string? status)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var query = _context.Appointments.AsQueryable();

        if (User.IsInRole("Patient"))
        {
            var patient =
                await GetCurrentPatientAsync(currentUserId);

            if (patient == null)
            {
                return Forbid();
            }

            query = query.Where(
                a => a.PatientId == patient.Id
            );
        }
        else if (User.IsInRole("Doctor"))
        {
            var doctor =
                await GetCurrentDoctorAsync(currentUserId);

            if (doctor == null)
            {
                return Forbid();
            }

            query = query.Where(
                a => a.DoctorId == doctor.Id
            );
        }
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Optional status filter.
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(
                a => a.Status == status
            );
        }

        var appointments = await query
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.DoctorId,
                a.AppointmentDate,
                a.Reason,
                a.Status
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // GET: api/appointments/1
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var appointment =
            await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new
            {
                message =
                    $"Appointment with ID {id} was not found."
            });
        }

        if (User.IsInRole("Admin"))
        {
            return Ok(appointment);
        }

        if (User.IsInRole("Patient"))
        {
            var patient =
                await GetCurrentPatientAsync(currentUserId);

            if (
                patient == null ||
                appointment.PatientId != patient.Id
            )
            {
                return Forbid();
            }

            return Ok(appointment);
        }

        if (User.IsInRole("Doctor"))
        {
            var doctor =
                await GetCurrentDoctorAsync(currentUserId);

            if (
                doctor == null ||
                appointment.DoctorId != doctor.Id
            )
            {
                return Forbid();
            }

            return Ok(appointment);
        }

        return Forbid();
    }

    // POST: api/appointments
    //
    // Patient:
    //     Can create an appointment only for themselves.
    //
    // Doctor:
    //     Can create an appointment only where they are
    //     the assigned Doctor.
    //
    // Admin:
    //     Can create any valid appointment.
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> Create(
        CreateAppointmentRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var patientId = request.PatientId;
        var doctorId = request.DoctorId;

        // Patient identity comes from the JWT,
        // not from PatientId supplied by the client.
        if (User.IsInRole("Patient"))
        {
            var patient =
                await GetCurrentPatientAsync(currentUserId);

            if (patient == null)
            {
                return Forbid();
            }

            patientId = patient.Id;
        }

        // Doctor identity comes from the JWT,
        // not from DoctorId supplied by the client.
        if (User.IsInRole("Doctor"))
        {
            var doctor =
                await GetCurrentDoctorAsync(currentUserId);

            if (doctor == null)
            {
                return Forbid();
            }

            doctorId = doctor.Id;
        }

        var patientExists =
            await _context.Patients.AnyAsync(
                p => p.Id == patientId
            );

        if (!patientExists)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {patientId} was not found."
            });
        }

        var doctorExists =
            await _context.Doctors.AnyAsync(
                d => d.Id == doctorId
            );

        if (!doctorExists)
        {
            return NotFound(new
            {
                message =
                    $"Doctor with ID {doctorId} was not found."
            });
        }

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = request.AppointmentDate,
            Reason = request.Reason,
            Status = request.Status
        };

        _context.Appointments.Add(appointment);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = appointment.Id },
            new
            {
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.AppointmentDate,
                appointment.Reason,
                appointment.Status
            }
        );
    }

    // PUT: api/appointments/1
    //
    // Patient:
    //     Can update only their own appointment.
    //
    // Doctor:
    //     Can update only appointments assigned to them.
    //
    // Admin:
    //     Can update any appointment.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAppointmentRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var appointment =
            await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new
            {
                message =
                    $"Appointment with ID {id} was not found."
            });
        }

        if (User.IsInRole("Patient"))
        {
            var patient =
                await GetCurrentPatientAsync(currentUserId);

            if (
                patient == null ||
                appointment.PatientId != patient.Id
            )
            {
                return Forbid();
            }

            // Patient remains the owner of the appointment.
            appointment.PatientId = patient.Id;

            // Patient can select a valid Doctor.
            var doctorExists =
                await _context.Doctors.AnyAsync(
                    d => d.Id == request.DoctorId
                );

            if (!doctorExists)
            {
                return NotFound(new
                {
                    message =
                        $"Doctor with ID {request.DoctorId} was not found."
                });
            }

            appointment.DoctorId = request.DoctorId;
        }
        else if (User.IsInRole("Doctor"))
        {
            var doctor =
                await GetCurrentDoctorAsync(currentUserId);

            if (
                doctor == null ||
                appointment.DoctorId != doctor.Id
            )
            {
                return Forbid();
            }

            var patientExists =
                await _context.Patients.AnyAsync(
                    p => p.Id == request.PatientId
                );

            if (!patientExists)
            {
                return NotFound(new
                {
                    message =
                        $"Patient with ID {request.PatientId} was not found."
                });
            }

            // Doctor remains the assigned Doctor.
            appointment.DoctorId = doctor.Id;
            appointment.PatientId = request.PatientId;
        }
        else if (User.IsInRole("Admin"))
        {
            var patientExists =
                await _context.Patients.AnyAsync(
                    p => p.Id == request.PatientId
                );

            if (!patientExists)
            {
                return NotFound(new
                {
                    message =
                        $"Patient with ID {request.PatientId} was not found."
                });
            }

            var doctorExists =
                await _context.Doctors.AnyAsync(
                    d => d.Id == request.DoctorId
                );

            if (!doctorExists)
            {
                return NotFound(new
                {
                    message =
                        $"Doctor with ID {request.DoctorId} was not found."
                });
            }

            appointment.PatientId = request.PatientId;
            appointment.DoctorId = request.DoctorId;
        }
        else
        {
            return Forbid();
        }

        appointment.AppointmentDate =
            request.AppointmentDate;

        appointment.Reason =
            request.Reason;

        appointment.Status =
            request.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/appointments/1
    //
    // Patient:
    //     Can delete only their own appointment.
    //
    // Doctor:
    //     Can delete only appointments assigned to them.
    //
    // Admin:
    //     Can delete any appointment.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var appointment =
            await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new
            {
                message =
                    $"Appointment with ID {id} was not found."
            });
        }

        if (User.IsInRole("Patient"))
        {
            var patient =
                await GetCurrentPatientAsync(currentUserId);

            if (
                patient == null ||
                appointment.PatientId != patient.Id
            )
            {
                return Forbid();
            }
        }
        else if (User.IsInRole("Doctor"))
        {
            var doctor =
                await GetCurrentDoctorAsync(currentUserId);

            if (
                doctor == null ||
                appointment.DoctorId != doctor.Id
            )
            {
                return Forbid();
            }
        }
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Appointments.Remove(appointment);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}