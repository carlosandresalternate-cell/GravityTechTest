using Microsoft.AspNetCore.Mvc;
using MediTrack.Api.DTOs;
using MediTrack.Api.Services;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Get all active doctors.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DoctorResponse>>> GetAll()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    /// <summary>
    /// Get a doctor by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DoctorResponse>> GetById(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound();
        return Ok(doctor);
    }

    /// <summary>
    /// Get all doctors with their upcoming appointment counts.
    /// </summary>
    [HttpGet("availability")]
    public async Task<ActionResult<List<DoctorWithAvailabilityResponse>>> GetWithAvailability()
    {
        var doctors = await _doctorService.GetDoctorsWithAvailabilityAsync();
        return Ok(doctors);
    }

    /// <summary>
    /// Search doctors by specialty and/or availability.
    /// TODO: Implement (Task 2b) — wire up to DoctorService.SearchDoctorsAsync
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<DoctorResponse>>> Search(
        [FromQuery] string? specialty,
        [FromQuery] DateTime? availableOn)
    {
        var availableDoctors = await _doctorService.SearchDoctorsAsync(specialty, availableOn);

        if (availableDoctors is null || availableDoctors.Count == 0)
        {
            return NotFound();
        }

        return Ok(availableDoctors);
    }
}
