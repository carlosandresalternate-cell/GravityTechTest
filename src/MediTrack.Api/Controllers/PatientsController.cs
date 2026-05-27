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

    // TODO: Implement all endpoints listed above
}
