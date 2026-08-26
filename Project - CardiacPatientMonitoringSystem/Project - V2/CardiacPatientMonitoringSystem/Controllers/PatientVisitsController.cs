using CardiacPatientMonitoringSystem.DTOs;
using CardiacPatientMonitoringSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardiacPatientMonitoringSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientVisitsController : ControllerBase
{
    private readonly PatientVisitService _patientVisitService;

    public PatientVisitsController(PatientVisitService patientVisitService)
    {
        _patientVisitService = patientVisitService;
    }

    // POST: api/patientvisits
    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientVisitRequest request)
    {
        var result = await _patientVisitService.CreateVisitAsync(request);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }
}