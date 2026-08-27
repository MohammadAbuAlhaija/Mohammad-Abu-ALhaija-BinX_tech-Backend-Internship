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
public class MedicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/medications
    // Admin, Doctor and Patient can view the medication catalog.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        var medications = await _context.Medications
            .ToListAsync();

        return Ok(medications);
    }

    // GET: api/medications/{id}
    // Admin, Doctor and Patient can view a medication.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var medication = await _context.Medications
            .FindAsync(id);

        if (medication == null)
        {
            return NotFound(new
            {
                message = $"Medication with ID {id} was not found."
            });
        }

        return Ok(medication);
    }

    // POST: api/medications
    // Only Admin can create medications.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateMedicationRequest request)
    {
        var medication = new Medication
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Medications.Add(medication);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = medication.Id },
            medication
        );
    }

    // PUT: api/medications/{id}
    // Only Admin can update medications.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdateMedicationRequest request)
    {
        var medication = await _context.Medications
            .FindAsync(id);

        if (medication == null)
        {
            return NotFound(new
            {
                message = $"Medication with ID {id} was not found."
            });
        }

        medication.Name = request.Name;
        medication.Description = request.Description;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/medications/{id}
    // Only Admin can delete medications.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var medication = await _context.Medications
            .FindAsync(id);

        if (medication == null)
        {
            return NotFound(new
            {
                message = $"Medication with ID {id} was not found."
            });
        }

        _context.Medications.Remove(medication);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}