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
public class PatientPhonesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientPhonesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/patientphones
    // Admin, Doctor and Patient can view patient phone records.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var phones = await _context.PatientPhones
            .Include(p => p.Patient)
            .ToListAsync();

        return Ok(phones);
    }

    // GET: api/patientphones/{id}
    // Admin, Doctor and Patient can view a patient phone record.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var phone = await _context.PatientPhones
            .Include(p => p.Patient)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient phone with ID {id} was not found."
            });
        }

        return Ok(phone);
    }

    // POST: api/patientphones
    // Only Admin can create patient phone records.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreatePatientPhoneRequest request)
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

        var phone = new PatientPhone
        {
            PatientId = request.PatientId,
            PhoneNumber = request.PhoneNumber,
            Type = request.Type
        };

        _context.PatientPhones.Add(phone);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = phone.Id },
            phone
        );
    }

    // PUT: api/patientphones/{id}
    // Only Admin can update patient phone records.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePatientPhoneRequest request)
    {
        var phone = await _context.PatientPhones
            .FindAsync(id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient phone with ID {id} was not found."
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

        phone.PatientId = request.PatientId;
        phone.PhoneNumber = request.PhoneNumber;
        phone.Type = request.Type;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/patientphones/{id}
    // Only Admin can delete patient phone records.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var phone = await _context.PatientPhones
            .FindAsync(id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Patient phone with ID {id} was not found."
            });
        }

        _context.PatientPhones.Remove(phone);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}