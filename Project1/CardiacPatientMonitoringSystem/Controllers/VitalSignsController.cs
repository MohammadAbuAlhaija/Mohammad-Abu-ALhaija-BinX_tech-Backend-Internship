using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VitalSignsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VitalSignsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/vitalsigns
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vitalSigns = await _context.VitalSigns.ToListAsync();

        return Ok(vitalSigns);
    }

    // GET: api/vitalsigns/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vitalSign = await _context.VitalSigns.FindAsync(id);

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message = $"Vital sign with ID {id} was not found."
            });
        }

        return Ok(vitalSign);
    }

    // POST: api/vitalsigns
    [HttpPost]
    public async Task<IActionResult> Create(CreateVitalSignRequest request)
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

        var vitalSign = new VitalSign
        {
            PatientId = request.PatientId,
            HeartRate = request.HeartRate,
            SystolicBloodPressure = request.SystolicBloodPressure,
            DiastolicBloodPressure = request.DiastolicBloodPressure,
            MeasuredAt = request.MeasuredAt
        };

        _context.VitalSigns.Add(vitalSign);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = vitalSign.Id },
            vitalSign
        );
    }

    // PUT: api/vitalsigns/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateVitalSignRequest request)
    {
        var vitalSign = await _context.VitalSigns.FindAsync(id);

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message = $"Vital sign with ID {id} was not found."
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

        vitalSign.PatientId = request.PatientId;
        vitalSign.HeartRate = request.HeartRate;
        vitalSign.SystolicBloodPressure = request.SystolicBloodPressure;
        vitalSign.DiastolicBloodPressure = request.DiastolicBloodPressure;
        vitalSign.MeasuredAt = request.MeasuredAt;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/vitalsigns/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vitalSign = await _context.VitalSigns.FindAsync(id);

        if (vitalSign == null)
        {
            return NotFound(new
            {
                message = $"Vital sign with ID {id} was not found."
            });
        }

        _context.VitalSigns.Remove(vitalSign);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}