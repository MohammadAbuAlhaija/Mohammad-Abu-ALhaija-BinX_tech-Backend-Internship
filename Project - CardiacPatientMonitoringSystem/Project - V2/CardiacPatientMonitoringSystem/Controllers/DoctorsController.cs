using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DoctorsController(
        AppDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // POST: api/doctors
    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            return NotFound(new
            {
                message = $"User with ID {request.UserId} was not found."
            });
        }

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.Id == request.DepartmentId);

        if (!departmentExists)
        {
            return NotFound(new
            {
                message = $"Department with ID {request.DepartmentId} was not found."
            });
        }

        if (request.SupervisorId.HasValue)
        {
            var supervisorExists = await _context.Doctors
                .AnyAsync(d => d.Id == request.SupervisorId.Value);

            if (!supervisorExists)
            {
                return NotFound(new
                {
                    message = $"Supervisor with ID {request.SupervisorId.Value} was not found."
                });
            }
        }

        var userAlreadyAssigned = await _context.Doctors
            .AnyAsync(d => d.UserId == request.UserId);

        if (userAlreadyAssigned)
        {
            return BadRequest(new
            {
                message = "This user is already assigned to a doctor."
            });
        }

        var doctor = new Doctor
        {
            UserId = request.UserId,
            DepartmentId = request.DepartmentId,
            SupervisorId = request.SupervisorId,
            FullName = request.FullName,
            Specialization = request.Specialization
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = doctor.Id },
            new
            {
                doctor.Id,
                doctor.UserId,
                doctor.DepartmentId,
                doctor.SupervisorId,
                doctor.FullName,
                doctor.Specialization
            }
        );
    }

    // GET: api/doctors/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _context.Doctors
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                d.UserId,
                d.DepartmentId,
                d.SupervisorId,
                d.FullName,
                d.Specialization
            })
            .FirstOrDefaultAsync();

        if (doctor == null)
        {
            return NotFound(new
            {
                message = $"Doctor with ID {id} was not found."
            });
        }

        return Ok(doctor);
    }
}