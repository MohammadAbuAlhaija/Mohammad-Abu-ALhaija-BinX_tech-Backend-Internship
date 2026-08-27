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
public class DoctorPhonesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorPhonesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/doctorphones
    // Admin and Doctor can view doctor phone records.
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAll()
    {
        var phones = await _context.DoctorPhones
            .Include(p => p.Doctor)
            .ToListAsync();

        return Ok(phones);
    }

    // GET: api/doctorphones/{id}
    // Admin and Doctor can view a doctor phone record.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetById(int id)
    {
        var phone = await _context.DoctorPhones
            .Include(p => p.Doctor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Doctor phone with ID {id} was not found."
            });
        }

        return Ok(phone);
    }

    // POST: api/doctorphones
    // Only Admin can create doctor phone records.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateDoctorPhoneRequest request)
    {
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId);

        if (!doctorExists)
        {
            return NotFound(new
            {
                message =
                    $"Doctor with ID {request.DoctorId} was not found."
            });
        }

        var phone = new DoctorPhone
        {
            DoctorId = request.DoctorId,
            PhoneNumber = request.PhoneNumber,
            Type = request.Type
        };

        _context.DoctorPhones.Add(phone);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = phone.Id },
            phone
        );
    }

    // PUT: api/doctorphones/{id}
    // Only Admin can update doctor phone records.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        UpdateDoctorPhoneRequest request)
    {
        var phone = await _context.DoctorPhones
            .FindAsync(id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Doctor phone with ID {id} was not found."
            });
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId);

        if (!doctorExists)
        {
            return NotFound(new
            {
                message =
                    $"Doctor with ID {request.DoctorId} was not found."
            });
        }

        phone.DoctorId = request.DoctorId;
        phone.PhoneNumber = request.PhoneNumber;
        phone.Type = request.Type;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/doctorphones/{id}
    // Only Admin can delete doctor phone records.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var phone = await _context.DoctorPhones
            .FindAsync(id);

        if (phone == null)
        {
            return NotFound(new
            {
                message =
                    $"Doctor phone with ID {id} was not found."
            });
        }

        _context.DoctorPhones.Remove(phone);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}