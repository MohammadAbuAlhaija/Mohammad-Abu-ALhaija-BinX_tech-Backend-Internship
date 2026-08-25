using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var medications = await _context.Medications.ToListAsync();

        return Ok(medications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medication = await _context.Medications.FindAsync(id);

        if (medication == null)
        {
            return NotFound(new
            {
                message = $"Medication with ID {id} was not found."
            });
        }

        return Ok(medication);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicationRequest request)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateMedicationRequest request)
    {
        var medication = await _context.Medications.FindAsync(id);

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medication = await _context.Medications.FindAsync(id);

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