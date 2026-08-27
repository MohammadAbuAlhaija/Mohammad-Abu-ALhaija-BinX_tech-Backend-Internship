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
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PatientsController(
        AppDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Gets the Identity UserId of the currently authenticated user
    // from the JWT token.
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // GET: api/patients
    //
    // Admin:
    //     Can browse all patients.
    //
    // Doctor:
    //     Can browse only patients connected to that doctor
    //     through Appointments or MedicalRecords.
    //
    // Patient:
    //     Can only receive their own patient record.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        string? gender = null,
        string? sort = null)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var query = _context.Patients.AsQueryable();

        // Patient can only see their own record.
        if (User.IsInRole("Patient"))
        {
            query = query.Where(
                p => p.UserId == currentUserId
            );
        }

        // Doctor can only see patients connected to them
        // through appointments or medical records.
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

            query = query.Where(p =>
                p.Appointments.Any(
                    a => a.DoctorId == doctor.Id
                )
                ||
                p.MedicalRecords.Any(
                    m => m.DoctorId == doctor.Id
                )
            );
        }

        // Admin receives the full Patients query.
        else if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // Optional filtering
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(
                p => p.FullName.Contains(name)
            );
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            query = query.Where(
                p => p.Gender == gender
            );
        }

        // Count after applying authorization and filters.
        var totalCount = await query.CountAsync();

        // Sorting
        query = sort switch
        {
            "name_desc" =>
                query.OrderByDescending(p => p.FullName),

            "birthdate_asc" =>
                query.OrderBy(p => p.DateOfBirth),

            "birthdate_desc" =>
                query.OrderByDescending(p => p.DateOfBirth),

            _ =>
                query.OrderBy(p => p.FullName)
        };

        // Pagination + DTO projection
        var patients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientResponse
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            data = patients
        });
    }

    // GET: api/patients/1
    //
    // Admin:
    //     Can access any patient.
    //
    // Doctor:
    //     Can only access a patient connected to them.
    //
    // Patient:
    //     Can only access their own patient record.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var patient = await _context.Patients
            .Where(p => p.Id == id)
            .Select(p => new PatientResponse
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender
            })
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound(new
            {
                message = $"Patient with ID {id} was not found."
            });
        }

        // Admin can access any existing patient.
        if (User.IsInRole("Admin"))
        {
            return Ok(patient);
        }

        // Patient can only access themselves.
        if (User.IsInRole("Patient"))
        {
            var ownsPatientRecord =
                await _context.Patients.AnyAsync(
                    p =>
                        p.Id == id &&
                        p.UserId == currentUserId
                );

            if (!ownsPatientRecord)
            {
                return Forbid();
            }

            return Ok(patient);
        }

        // Doctor can only access patients connected to them.
        if (User.IsInRole("Doctor"))
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(
                    d => d.UserId == currentUserId
                );

            if (doctor == null)
            {
                return Forbid();
            }

            var hasAccess =
                await _context.Patients.AnyAsync(
                    p =>
                        p.Id == id &&
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

            if (!hasAccess)
            {
                return Forbid();
            }

            return Ok(patient);
        }

        return Forbid();
    }

    // POST: api/patients
    //
    // Only Admin can manually create a Patient record.
    // Normal Patient registration should use:
    // POST /api/auth/register/patient
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePatientRequest request)
    {
        // The supplied Identity user must exist.
        var user =
            await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message =
                    $"User with ID {request.UserId} was not found."
            });
        }

        // Prevent assigning a Doctor/Admin account
        // as a Patient simply by supplying its UserId.
        var isPatientUser =
            await _userManager.IsInRoleAsync(
                user,
                "Patient"
            );

        if (!isPatientUser)
        {
            return BadRequest(new
            {
                message =
                    "The selected Identity user does not have the Patient role."
            });
        }

        // Prevent one Identity account from being linked
        // to multiple Patient records.
        var patientAlreadyExists =
            await _context.Patients.AnyAsync(
                p => p.UserId == request.UserId
            );

        if (patientAlreadyExists)
        {
            return BadRequest(new
            {
                message =
                    "This user is already assigned to a patient."
            });
        }

        var patient = new Patient
        {
            UserId = request.UserId,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender
        };

        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = patient.Id },
            new PatientResponse
            {
                Id = patient.Id,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender
            }
        );
    }

    // PUT: api/patients/1
    //
    // Admin can update any patient.
    // Patient can update only their own data.
    // Doctor cannot modify Patient profile information.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Patient")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePatientRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        var patient =
            await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {id} was not found."
            });
        }

        // A Patient cannot update another Patient's profile.
        if (
            User.IsInRole("Patient") &&
            patient.UserId != currentUserId
        )
        {
            return Forbid();
        }

        patient.FullName = request.FullName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/patients/1
    //
    // Only Admin can delete Patient records.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient =
            await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient with ID {id} was not found."
            });
        }

        _context.Patients.Remove(patient);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}