using CardiacPatientMonitoringSystem.Data;
using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthController(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        AppDbContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    // Register Patient
    [HttpPost("register/patient")]
    public async Task<IActionResult> RegisterPatient(
        RegisterPatientRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "User already exists."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var createUserResult =
                await _userManager.CreateAsync(
                    user,
                    request.Password
                );

            if (!createUserResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(createUserResult.Errors);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Patient"
                );

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                return BadRequest(roleResult.Errors);
            }

            var patient = new Patient
            {
                UserId = user.Id,
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender
            };

            _context.Patients.Add(patient);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return StatusCode(201, new
            {
                message = "Patient registered successfully.",
                patientId = patient.Id
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Register Doctor
    [HttpPost("register/doctor")]
    public async Task<IActionResult> RegisterDoctor(
        RegisterDoctorRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "User already exists."
            });
        }

        var departmentExists =
            await _context.Departments
                .AnyAsync(d => d.Id == request.DepartmentId);

        if (!departmentExists)
        {
            return NotFound(new
            {
                message =
                    $"Department with ID {request.DepartmentId} was not found."
            });
        }

        if (request.SupervisorId.HasValue)
        {
            var supervisorExists =
                await _context.Doctors
                    .AnyAsync(
                        d => d.Id == request.SupervisorId.Value
                    );

            if (!supervisorExists)
            {
                return NotFound(new
                {
                    message =
                        $"Supervisor with ID {request.SupervisorId.Value} was not found."
                });
            }
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var createUserResult =
            await _userManager.CreateAsync(
                user,
                request.Password
            );

        if (!createUserResult.Succeeded)
        {
            return BadRequest(createUserResult.Errors);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "Doctor"
            );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(roleResult.Errors);
        }

        var doctor = new Doctor
        {
            UserId = user.Id,
            DepartmentId = request.DepartmentId,
            SupervisorId = request.SupervisorId,
            FullName = request.FullName,
            Specialization = request.Specialization
        };

        _context.Doctors.Add(doctor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return StatusCode(201, new
        {
            message = "Doctor registered successfully.",
            doctorId = doctor.Id
        });
    }

    // Register Admin
    // Temporary bootstrap endpoint for creating the first Admin account.
    [HttpPost("register/admin")]
    public async Task<IActionResult> RegisterAdmin(
        RegisterAdminRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "User already exists."
            });
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var createUserResult =
            await _userManager.CreateAsync(
                user,
                request.Password
            );

        if (!createUserResult.Succeeded)
        {
            return BadRequest(createUserResult.Errors);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "Admin"
            );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(roleResult.Errors);
        }

        return StatusCode(201, new
        {
            message = "Admin registered successfully."
        });
    }

    // Login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        string email,
        string password)
    {
        var user =
            await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                password
            );

        if (!passwordValid)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id
            ),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email!
            )
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role
                )
            );
        }

        // Add PatientId claim for Patient users
        if (roles.Contains("Patient"))
        {
            var patient =
                await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => p.UserId == user.Id
                    );

            if (patient == null)
            {
                return Unauthorized(new
                {
                    message =
                        "Patient profile not found for this account."
                });
            }

            claims.Add(
                new Claim(
                    "PatientId",
                    patient.Id.ToString()
                )
            );
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

        var token =
            new JwtSecurityToken(
                issuer:
                    _configuration["Jwt:Issuer"],

                audience:
                    _configuration["Jwt:Audience"],

                claims:
                    claims,

                expires:
                    DateTime.UtcNow.AddMinutes(30),

                signingCredentials:
                    credentials
            );

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            roles,
            expiresInMinutes = 30
        });
    }
}

