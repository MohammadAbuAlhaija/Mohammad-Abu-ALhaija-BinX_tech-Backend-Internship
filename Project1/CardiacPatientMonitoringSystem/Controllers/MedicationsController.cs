using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message = $"Patient with ID {request.PatientId} was not found."
            });
        }

        var medication = new Medication
        {
            PatientId = request.PatientId,
            Name = request.Name,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate
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

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message = $"Patient with ID {request.PatientId} was not found."
            });
        }

        medication.PatientId = request.PatientId;
        medication.Name = request.Name;
        medication.Dosage = request.Dosage;
        medication.Frequency = request.Frequency;
        medication.StartDate = request.StartDate;
        medication.EndDate = request.EndDate;

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