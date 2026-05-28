using Microsoft.AspNetCore.Mvc;
using MediTrack.Api.DTOs;
using MediTrack.Api.Services;

namespace MediTrack.Api.Controllers;

/// <summary>
/// TODO (Task 2a): Implement this controller.
/// 
/// Required endpoints:
///   GET    /api/patients              — List patients (paginated: page, pageSize query params)
///   GET    /api/patients/{id}         — Get patient by ID (include appointment history)
///   POST   /api/patients              — Create a new patient
///   PUT    /api/patients/{id}         — Update patient details
///   DELETE /api/patients/{id}         — Soft-delete (set IsActive = false)
/// 
/// Guidelines:
///   - Use proper HTTP status codes (200, 201, 204, 400, 404)
///   - Use [FromBody], [FromQuery], [FromRoute] attributes correctly
///   - Wire up to IPatientService methods
///   - Default pagination: page=1, pageSize=10
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("Page and pageSize must be greater than zero.");
        }

        var response = await _patientService.GetPatientsAsync(page, pageSize);

        return Ok(response?.Items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);

        return Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest dto)
    {
        var createdPatient = await _patientService.CreatePatientAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest dto)
    {
        var updated = await _patientService.UpdatePatientAsync(id, dto);
        if (updated is null)
        {
            return NotFound($"Patient with ID {id} not found.");
        }
        return Ok(updated); //successful PUT returning updated content
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _patientService.DeletePatientAsync(id);
        if (!deleted)
        {
            return NotFound($"Patient with ID {id} not found.");
        }
        return Ok("Patient deleted successfully."); //use 200 to confirm deletion
    }
}
