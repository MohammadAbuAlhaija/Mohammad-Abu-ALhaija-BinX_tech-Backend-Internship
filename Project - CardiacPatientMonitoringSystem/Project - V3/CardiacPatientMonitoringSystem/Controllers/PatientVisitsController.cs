using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientVisitsController : ControllerBase
{
    private readonly PatientVisitService _patientVisitService;
    private readonly AppDbContext _context;

    public PatientVisitsController(
        PatientVisitService patientVisitService,
        AppDbContext context)
    {
        _patientVisitService = patientVisitService;
        _context = context;
    }

    // Gets the Identity UserId of the authenticated user
    // from the JWT token.
    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // POST: api/patientvisits
    //
    // Doctor:
    //     Can create a Patient Visit, but the DoctorId
    //     is always taken from the authenticated account.
    //
    // Admin:
    //     Can create a visit for any valid Doctor and Patient.
    //
    // Patient:
    //     Cannot create medical visits.
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create(
        CreatePatientVisitRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        // If the authenticated user is a Doctor,
        // never trust DoctorId sent in the request body.
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

            // Force the visit to belong to the
            // authenticated Doctor.
            request.DoctorId = doctor.Id;
        }

        var result =
            await _patientVisitService.CreateVisitAsync(request);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }
}