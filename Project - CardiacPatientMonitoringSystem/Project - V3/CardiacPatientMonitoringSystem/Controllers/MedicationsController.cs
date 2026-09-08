using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;

    private const string MedicationsCacheKey = "medications:all";

    public MedicationsController(
        AppDbContext context,
        IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: api/medications
    // Admin, Doctor and Patient can view the medication catalog.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> GetAll()
    {
        // Try to get medications from Redis cache.
        var cachedData =
            await _cache.GetStringAsync(MedicationsCacheKey);

        if (cachedData is not null)
        {
            var cachedMedications =
                JsonSerializer.Deserialize<List<Medication>>(cachedData);

            if (cachedMedications is not null)
            {
                return Ok(cachedMedications);
            }
        }

        // Cache miss: get medications from the database.
        var medications = await _context.Medications
            .AsNoTracking()
            .ToListAsync();

        // Store the result in Redis for 10 minutes.
        var serializedMedications =
            JsonSerializer.Serialize(medications);

        await _cache.SetStringAsync(
            MedicationsCacheKey,
            serializedMedications,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(10)
            });

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

        // Invalidate the medication catalog cache.
        await _cache.RemoveAsync(MedicationsCacheKey);

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

        // Invalidate the medication catalog cache.
        await _cache.RemoveAsync(MedicationsCacheKey);

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

        // Invalidate the medication catalog cache.
        await _cache.RemoveAsync(MedicationsCacheKey);

        return NoContent();
    }
}