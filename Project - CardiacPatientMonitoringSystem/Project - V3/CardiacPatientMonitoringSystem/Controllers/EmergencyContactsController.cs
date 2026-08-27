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
public class EmergencyContactsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmergencyContactsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/emergencycontacts
    // Admin, Doctor and Patient can view emergency contacts.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _context.EmergencyContacts
            .ToListAsync();

        return Ok(contacts);
    }

    // GET: api/emergencycontacts/{id}
    // Admin, Doctor and Patient can view an emergency contact.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var contact = await _context.EmergencyContacts
            .FindAsync(id);

        if (contact == null)
        {
            return NotFound(new
            {
                message =
                    $"Emergency contact with ID {id} was not found."
            });
        }

        return Ok(contact);
    }

    // POST: api/emergencycontacts
    // Only Admin can create emergency contacts.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateEmergencyContactRequest request)
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

        var contact = new EmergencyContact
        {
            PatientId = request.PatientId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            Relationship = request.Relationship
        };

        _context.EmergencyContacts.Add(contact);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = contact.Id },
            contact
        );
    }

    // PUT: api/emergencycontacts/{id}
    // Only Admin can update emergency contacts.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdateEmergencyContactRequest request)
    {
        var contact = await _context.EmergencyContacts
            .FindAsync(id);

        if (contact == null)
        {
            return NotFound(new
            {
                message =
                    $"Emergency contact with ID {id} was not found."
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

        contact.PatientId = request.PatientId;
        contact.Name = request.Name;
        contact.PhoneNumber = request.PhoneNumber;
        contact.Relationship = request.Relationship;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/emergencycontacts/{id}
    // Only Admin can delete emergency contacts.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _context.EmergencyContacts
            .FindAsync(id);

        if (contact == null)
        {
            return NotFound(new
            {
                message =
                    $"Emergency contact with ID {id} was not found."
            });
        }

        _context.EmergencyContacts.Remove(contact);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}