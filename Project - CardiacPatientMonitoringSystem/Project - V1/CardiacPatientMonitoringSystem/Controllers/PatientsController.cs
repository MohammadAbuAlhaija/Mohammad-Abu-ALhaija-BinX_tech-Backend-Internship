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
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

// GET: api/patients
[HttpGet]
public async Task<IActionResult> GetAll(
    int page = 1,
    int pageSize = 10,
    string? name = null,
    string? gender = null,
    string? sort = null)
{
    var query = _context.Patients.AsQueryable();

    // Filtering
    if (!string.IsNullOrWhiteSpace(name))
    {
        query = query.Where(p => p.FullName.Contains(name));
    }

    if (!string.IsNullOrWhiteSpace(gender))
    {
        query = query.Where(p => p.Gender == gender);
    }

    var totalCount = await query.CountAsync();

    // Sorting
    query = sort switch
    {
        "name_desc" => query.OrderByDescending(p => p.FullName),
        "birthdate_asc" => query.OrderBy(p => p.DateOfBirth),
        "birthdate_desc" => query.OrderByDescending(p => p.DateOfBirth),
        _ => query.OrderBy(p => p.FullName)
    };

    // Pagination + DTO Projection
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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message = $"Patient with ID {id} was not found."
            });
        }

        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientRequest request)
    {
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
            patient
        );
    }

    // PUT: api/patients/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePatientRequest request)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message = $"Patient with ID {id} was not found."
            });
        }

        patient.FullName = request.FullName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/patients/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message = $"Patient with ID {id} was not found."
            });
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}